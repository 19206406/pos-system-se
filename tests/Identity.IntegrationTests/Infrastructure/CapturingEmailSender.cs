using Identity.Application.Contracts.Messaging;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Identity.IntegrationTests.Infrastructure
{
    /// <summary> Keeps every email instead of sending it, and can make delivery fail on purpose for one address. </summary>
    public sealed partial class CapturingEmailSender : IEmailSender
    {
        private readonly ConcurrentQueue<EmailMessage> _sent = new();
        private readonly ConcurrentDictionary<string, int> _failuresLeft = new(StringComparer.OrdinalIgnoreCase);

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            if (_failuresLeft.TryGetValue(message.ToAddress, out var left) && left > 0)
            {
                _failuresLeft[message.ToAddress] = left - 1;
                throw new InvalidOperationException("Simulated SMTP failure");
            }

            _sent.Enqueue(message);
            return Task.CompletedTask;
        }

        public void FailNextDeliveries(string toAddress, int count) => _failuresLeft[toAddress] = count;

        public IReadOnlyList<EmailMessage> SentTo(string toAddress) =>
            _sent.Where(m => string.Equals(m.ToAddress, toAddress, StringComparison.OrdinalIgnoreCase)).ToList();

        /// <summary> Waits for the outbox processor (polling every second in tests) to deliver an email to the address. </summary>
        public async Task<EmailMessage> WaitForEmailAsync(
            string toAddress, int expectedCount = 1, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(30));

            while (DateTime.UtcNow < deadline)
            {
                var messages = SentTo(toAddress);
                if (messages.Count >= expectedCount)
                    return messages[expectedCount - 1];

                await Task.Delay(100);
            }

            throw new TimeoutException($"Expected {expectedCount} email(s) to {toAddress}; got {SentTo(toAddress).Count}.");
        }

        public static string ExtractToken(EmailMessage message)
        {
            var match = TokenInLink().Match(message.Body);
            Assert.True(match.Success, "The email does not contain a set-password link.");
            return Uri.UnescapeDataString(match.Groups["token"].Value);
        }

        [GeneratedRegex(@"set-password\?token=(?<token>\S+)")]
        private static partial Regex TokenInLink();
    }
}
