using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Privy
{
    internal class RpcProvider : IRpcProvider
    {
        public int HdWalletIndex { get; }

        private readonly WalletEntropy _entropy;

        private EmbeddedWalletManager _embeddedWalletManager;

        private static readonly HashSet<string> _allowedMethods = new HashSet<string>
        {
            "eth_sign",
            "personal_sign",
            "eth_populateTransactionRequest",
            "eth_signTypedData_v4",
            "eth_signTransaction",
            "eth_sendTransaction"
        };

        public RpcProvider(WalletEntropy walletEntropy, int hdWalletIndex, EmbeddedWalletManager embeddedWalletManager)
        {
            this.HdWalletIndex = hdWalletIndex;
            _embeddedWalletManager = embeddedWalletManager;
            _entropy = walletEntropy;
        }

        public async Task<RpcResponse> Request(RpcRequest request)
        {
            if (_allowedMethods.Contains(request.Method))
            {
                var requestDetails = new RpcRequestData.EthereumRpcRequestDetails
                {
                    Method = request.Method,
                    Params = request.Params
                };
                var responseDetails =
                    await _embeddedWalletManager.Request(_entropy, ChainType.Ethereum, HdWalletIndex, requestDetails);

                if (responseDetails is RpcResponseData.EthereumRpcResponseDetails response)
                {
                    return new RpcResponse
                    {
                        Method = response.Method,
                        Data = response.Data
                    };
                }

                throw new PrivyException.EmbeddedWalletException($"Failed to execute RPC Request",
                    EmbeddedWalletError.RpcRequestFailed);
            }
            else
            {
                return await HandleJsonRpc(request);
            }
        }

        private async Task<RpcResponse> HandleJsonRpc(RpcRequest request)
        {
            PrivyLogger.Debug("Unsupported rpc request type");
            return null;
        }
    }
}