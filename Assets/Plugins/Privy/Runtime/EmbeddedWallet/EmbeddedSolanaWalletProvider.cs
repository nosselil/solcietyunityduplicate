using System.Threading.Tasks;

namespace Privy
{
    internal class EmbeddedSolanaWalletProvider : IEmbeddedSolanaWalletProvider
    {
        private readonly EmbeddedWalletManager _embeddedWalletManager;
        private readonly int _hdWalletIndex;
        private readonly WalletEntropy _walletEntropy;

        internal EmbeddedSolanaWalletProvider(WalletEntropy walletEntropy, PrivyEmbeddedSolanaWalletAccount account,
            EmbeddedWalletManager embeddedWalletManager)
        {
            _walletEntropy = walletEntropy;
            _hdWalletIndex = account.WalletIndex;
            _embeddedWalletManager = embeddedWalletManager;
        }

        public async Task<string> SignMessage(string message)
        {
            var request = new RpcRequestData.SolanaRpcRequestDetails
            {
                Method = "signMessage",
                Params = new RpcRequestData.SolanaSignMessageRpcRequestParams { Message = message }
            };

            var response =
                await _embeddedWalletManager.Request(_walletEntropy, ChainType.Solana, _hdWalletIndex, request);

            if (response is RpcResponseData.SolanaRpcResponseDetails signatureRespose)
                return signatureRespose.Data.Signature;

            throw new PrivyException.EmbeddedWalletException("Failed to execute message signature",
                EmbeddedWalletError.RpcRequestFailed);
        }
    }
}