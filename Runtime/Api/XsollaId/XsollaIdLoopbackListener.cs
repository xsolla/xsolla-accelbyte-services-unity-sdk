// Copyright (c) 2026 Xsolla Inc. All Rights Reserved.
// This is licensed software from Xsolla Inc. Powered by AccelByte.
// For limitation and restriction, contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xsolla.AccelByte
{
    /// <summary>
    /// Minimal HTTP listener on the IPv4 loopback address that waits for one OAuth redirect (RFC 8252 section 7.3).
    /// </summary>
    internal sealed class XsollaIdLoopbackListener : IDisposable
    {
        private const int MaxRequestBytes = 16 * 1024;
        private const int ConnectionTimeoutMilliseconds = 10000;

        private const string SuccessPage = "<!doctype html><html><head><meta charset=\"utf-8\"><title>Xsolla ID</title></head>"
            + "<body style=\"font-family:sans-serif;text-align:center;margin-top:4em\"><h2>Login complete</h2>"
            + "<p>You can close this window and return to the game.</p></body></html>";
        private const string FailurePage = "<!doctype html><html><head><meta charset=\"utf-8\"><title>Xsolla ID</title></head>"
            + "<body style=\"font-family:sans-serif;text-align:center;margin-top:4em\"><h2>Login failed</h2>"
            + "<p>Close this window and try again in the game.</p></body></html>";

        private readonly TaskCompletionSource<Dictionary<string, string>> callback =
            new TaskCompletionSource<Dictionary<string, string>>(TaskCreationOptions.RunContinuationsAsynchronously);

        private TcpListener listener;
        private string redirectPath;
        private Func<Dictionary<string, string>, bool> isSuccessfulCallback;
        private bool disposed;

        public int Port { get; private set; }

        /// <summary>Binds to 127.0.0.1, not localhost, so the redirect cannot reach another interface (RFC 8252 section 8.3).</summary>
        public void Start(int port, string path, Func<Dictionary<string, string>, bool> isSuccessful)
        {
            redirectPath = path;
            isSuccessfulCallback = isSuccessful;
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            _ = AcceptLoopAsync();
        }

        /// <summary>Completes with the query parameters of the first request to the redirect path.</summary>
        public Task<Dictionary<string, string>> WaitForCallbackAsync()
        {
            return callback.Task;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            try
            {
                listener?.Stop();
            }
            catch (SocketException)
            {
            }
            callback.TrySetCanceled();
        }

        private async Task AcceptLoopAsync()
        {
            while (!disposed)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                }
                catch (Exception) when (disposed)
                {
                    return;
                }
                catch (Exception exception)
                {
                    callback.TrySetException(exception);
                    return;
                }

                // Browsers open speculative connections that may never send a request, so each connection is handled on its own.
                _ = HandleConnectionAsync(client);
            }
        }

        private async Task HandleConnectionAsync(TcpClient client)
        {
            using (client)
            using (var timeout = new CancellationTokenSource(ConnectionTimeoutMilliseconds))
            using (timeout.Token.Register(client.Close))
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    string requestHead = await ReadRequestHeadAsync(stream).ConfigureAwait(false);
                    if (requestHead == null)
                    {
                        return;
                    }

                    string requestLine = requestHead.Split(new[] { "\r\n" }, StringSplitOptions.None)[0];
                    if (!XsollaIdProtocol.TryParseRequestLine(requestLine, out string method, out string path, out string query) || method != "GET")
                    {
                        await WriteResponseAsync(stream, "405 Method Not Allowed", FailurePage).ConfigureAwait(false);
                        return;
                    }

                    if (path != redirectPath)
                    {
                        // For example favicon.ico. Keep waiting for the real callback.
                        await WriteResponseAsync(stream, "404 Not Found", string.Empty).ConfigureAwait(false);
                        return;
                    }

                    Dictionary<string, string> parameters = XsollaIdProtocol.ParseQuery(query);
                    bool isSuccessful = isSuccessfulCallback == null || isSuccessfulCallback(parameters);
                    await WriteResponseAsync(stream, isSuccessful ? "200 OK" : "400 Bad Request", isSuccessful ? SuccessPage : FailurePage).ConfigureAwait(false);
                    callback.TrySetResult(parameters);
                }
                catch (Exception)
                {
                    // A broken or timed-out connection is not the callback. Keep waiting for the next one.
                }
            }
        }

        private static async Task<string> ReadRequestHeadAsync(NetworkStream stream)
        {
            var buffer = new byte[2048];
            var received = new StringBuilder();
            int total = 0;
            while (total < MaxRequestBytes)
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (read <= 0)
                {
                    return null;
                }
                total += read;
                received.Append(Encoding.ASCII.GetString(buffer, 0, read));
                string text = received.ToString();
                if (text.Contains("\r\n\r\n"))
                {
                    return text;
                }
            }
            return null;
        }

        private static async Task WriteResponseAsync(NetworkStream stream, string status, string body)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
            string head = $"HTTP/1.1 {status}\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {bodyBytes.Length}\r\n"
                + "Cache-Control: no-store\r\nConnection: close\r\n\r\n";
            byte[] headBytes = Encoding.ASCII.GetBytes(head);
            await stream.WriteAsync(headBytes, 0, headBytes.Length).ConfigureAwait(false);
            await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }
    }
}
