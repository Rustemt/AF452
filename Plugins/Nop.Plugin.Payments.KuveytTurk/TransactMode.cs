namespace Nop.Plugin.Payments.KuveytTurk
{
    /// <summary>
    /// Represents KuveytTurk payment processor transaction mode
    /// </summary>
    public enum TransactMode : int
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 0,
        /// <summary>
        /// Authorize
        /// </summary>
        Authorize = 1,
        /// <summary>
        /// Authorize and capture
        /// </summary>
        AuthorizeAndCapture= 2
    }
}
