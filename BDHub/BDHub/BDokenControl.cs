using System.Threading.Tasks;
using Nethereum.Geth;
using Nethereum.Web3.Accounts.Managed;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using BDHub.Models;
using System.Linq;

namespace BDHub
{
    public class BDokenControl
    {
        readonly string contractAddress = "0xfC26eBaB1AE33fB5c035c5f2AC40DD018D6CCA4B";
        readonly string abi = @"[{""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""PayUp"",""outputs"":[{""name"":""success"",""type"":""bool""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""newSellPrice"",""type"":""uint256""},{""name"":""newBuyPrice"",""type"":""uint256""}],""name"":""SetPrices"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""CheckBalance"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""sellPrice"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""amount"",""type"":""uint256""}],""name"":""Sell"",""outputs"":[{""name"":""revenue"",""type"":""uint256""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":"""",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""buyPrice"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""owner"",""outputs"":[{""name"":"""",""type"":""address""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""BloodForTheBloodGod"",""outputs"":[{""name"":""success"",""type"":""bool""}],""payable"":true,""stateMutability"":""payable"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""Buy"",""outputs"":[{""name"":""amount"",""type"":""uint256""}],""payable"":true,""stateMutability"":""payable"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_value"",""type"":""uint256""}],""name"":""CheckRequiredFunds"",""outputs"":[{""name"":""enough"",""type"":""bool""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""newOwner"",""type"":""address""}],""name"":""TransferOwnership"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""target"",""type"":""address""},{""name"":""mintedAmount"",""type"":""uint256""}],""name"":""MintToken"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""name"":""initialSupply"",""type"":""uint256""},{""name"":""tokenName"",""type"":""string""},{""name"":""tokenSymbol"",""type"":""string""},{""name"":""centralMinter"",""type"":""address""},{""name"":""sellPr"",""type"":""uint256""},{""name"":""buyPr"",""type"":""uint256""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""from"",""type"":""address""},{""indexed"":true,""name"":""to"",""type"":""address""},{""indexed"":false,""name"":""value"",""type"":""uint256""}],""name"":""TransferEvent"",""type"":""event""}]";
        readonly string testnetURL = "http://192.168.21.50:52353";

        private BDEntities db = new BDEntities();
        
        //Create new account, returns account address
        //Triba se riješit plaintexta
        public async Task<string> CreateNew(string password)
        {
            var web3 = new Web3Geth();
            string newAddress = await web3.Personal.NewAccount.SendRequestAsync(password);

            string head = (from h in db.CertUsers
                       where h.certUserID == 1
                       select h.beternumAddress).ToString();
            await MintToken(head, "password", newAddress, 10000000000000000000);
            return newAddress;
        }
        //ContractToNethereum function parser
        public Function GetFunction(string senderAddress, string password, string contractFunction)
        {
            ManagedAccount account = new ManagedAccount(senderAddress, password);
            Web3Geth web3 = new Web3Geth(account, testnetURL);
            Contract contract = web3.Eth.GetContract(abi, contractAddress);
            return contract.GetFunction(contractFunction);
        }
        //CheckBalance
        public async Task<BigInteger> CallFunction(string senderAddress, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null);
            return await function.CallAsync<BigInteger>(senderAddress, gas, null);
        }
        //Buy & Sell
        public async Task<BigInteger> CallFunction(string senderAddress, BigInteger value, Function function, int soCSharpKnowsDifference)
        {
            if (soCSharpKnowsDifference == 0)
            {
                HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null);
                return await function.CallAsync<BigInteger>(senderAddress, gas, value);
            }
            else if (soCSharpKnowsDifference == 1)
            {
                HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, value);
                return await function.CallAsync<BigInteger>(senderAddress, gas, null, value);
            }
            else
                return -1;
        }
        //CheckRequiredFunds
        public async Task<bool> CallFunction(string senderAddress, BigInteger value, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, value);
            return await function.CallAsync<bool>(senderAddress, gas, null, value);
        }
        //Transfer & MintToken
        public async Task CallFunction(string senderAddress, string receiverAddress, BigInteger value, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, receiverAddress, value);
            await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, receiverAddress, value);
        }
        //SetPrice
        public async Task CallFunction(string senderAddress, BigInteger sellPrice, BigInteger buyPrice, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, sellPrice, buyPrice);
            await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, sellPrice, buyPrice);
        }



        public async Task Transfer(string senderAddress, string password, string receiverAddress, BigInteger value)
        {
            Function transfer = GetFunction(senderAddress, password, "PayUp");
            await CallFunction(senderAddress, receiverAddress, value, transfer);
        }

        public async Task MintToken(string senderAddress, string password, string receiverAddress, BigInteger value)
        {
            Function mintToken = GetFunction(senderAddress, password, "MintToken");
            await CallFunction(senderAddress, receiverAddress, value, mintToken);
        }

        public async Task<BigInteger> CheckBalance(string senderAddress, string password)
        {
            Function checkBalance = GetFunction(senderAddress, password, "CheckBalance");
            return await CallFunction(senderAddress, checkBalance);
        }

        public async Task<bool> CheckRequiredFunds(string senderAddress, string password, BigInteger value)
        {
            Function checkRequiredFunds = GetFunction(senderAddress, password, "CheckRequiredFunds");
            return await CallFunction(senderAddress, value, checkRequiredFunds);
        }

        public async Task SetPrices(string senderAddress, string password, BigInteger sellPrice, BigInteger buyPrice)
        {
            Function setPrices = GetFunction(senderAddress, password, "SetPrices");
            await CallFunction(senderAddress, sellPrice, buyPrice, setPrices);
        }

        public async Task<BigInteger> Buy(string senderAddress, string password, BigInteger valueInETH)
        {
            Function buy = GetFunction(senderAddress, password, "Buy");
            return await CallFunction(senderAddress, valueInETH, buy, 0);
        }

        public async Task<BigInteger> Sell(string senderAddress, string password, BigInteger valueInBDWei)
        {
            Function sell = GetFunction(senderAddress, password, "Sell");
            return await CallFunction(senderAddress, valueInBDWei, sell, 1);
        }

        public async Task<bool> BloodForTheBloodGod(string senderAddress, string password, BigInteger valueInETH)
        {
            Function giveBlood = GetFunction(senderAddress, password, "BloodForTheBloodGod");
            HexBigInteger gas = await giveBlood.EstimateGasAsync(senderAddress, null, valueInETH);
            return await giveBlood.CallAsync<bool>(senderAddress, gas, valueInETH);
        }
    }
}