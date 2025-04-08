public static class WalletUtilities
{
    // Helper method to shorten a wallet address.
    public static string ShortenWalletAddress(string walletAddress)
    {
        if (string.IsNullOrEmpty(walletAddress) || walletAddress.Length <= 10)
            return walletAddress;

        // Get the first 5 characters and the last 3 characters.
        string firstPart = walletAddress.Substring(0, 5);
        string lastPart = walletAddress.Substring(walletAddress.Length - 3);
        return firstPart + ".." + lastPart;
    }
}