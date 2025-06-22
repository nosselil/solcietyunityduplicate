using System;

namespace Privy
{
    internal class EmbeddedSolanaWallet : IEmbeddedSolanaWallet
    {
        public string Address { get; }
        public string RecoveryMethod { get; }
        public int WalletIndex { get; }

        public IEmbeddedSolanaWalletProvider EmbeddedSolanaWalletProvider { get; }

        private EmbeddedSolanaWallet(PrivyEmbeddedSolanaWalletAccount account,
            IEmbeddedSolanaWalletProvider embeddedSolanaWalletProvider)
        {
            Address = account.Address;
            RecoveryMethod = account.RecoveryMethod;
            WalletIndex = account.WalletIndex;
            EmbeddedSolanaWalletProvider = embeddedSolanaWalletProvider;
        }

        internal static EmbeddedSolanaWallet Create(PrivyEmbeddedSolanaWalletAccount account,
            WalletEntropy walletEntropy, EmbeddedWalletManager embeddedWalletManager)
        {
            var embeddedSolanaWalletProvider =
                new EmbeddedSolanaWalletProvider(walletEntropy, account, embeddedWalletManager);

            return new EmbeddedSolanaWallet(account, embeddedSolanaWalletProvider);
        }
    }
}