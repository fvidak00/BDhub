using System.Threading.Tasks;
using Nethereum.Geth;
using Nethereum.Web3.Accounts.Managed;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using Nethereum.KeyStore;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using BDHub.Models;

namespace BDHub
{
    public class BDokenControl
    {
        private static BDEntities db = new BDEntities();
#pragma warning disable IDE0044 // Add readonly modifier
        private static string contractAddress = (from ca in db.BDokenDetails
                                                 select ca.BDID).ToList()[0].ToString();

        readonly string abi = @"[{""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""PayUp"",""outputs"":[{""name"":""success"",""type"":""bool""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""newSellPrice"",""type"":""uint256""},{""name"":""newBuyPrice"",""type"":""uint256""}],""name"":""SetPrices"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""CheckBalance"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""sellPrice"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""amount"",""type"":""uint256""}],""name"":""Sell"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":"""",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""buyPrice"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""GetSellPrice"",""outputs"":[{""name"":""price"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""owner"",""outputs"":[{""name"":"""",""type"":""address""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""BloodForTheBloodGod"",""outputs"":[{""name"":""success"",""type"":""bool""}],""payable"":true,""stateMutability"":""payable"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""Buy"",""outputs"":[],""payable"":true,""stateMutability"":""payable"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_value"",""type"":""uint256""}],""name"":""CheckRequiredFunds"",""outputs"":[{""name"":""enough"",""type"":""bool""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""GetBuyPrice"",""outputs"":[{""name"":""price"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""newOwner"",""type"":""address""}],""name"":""TransferOwnership"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""target"",""type"":""address""},{""name"":""mintedAmount"",""type"":""uint256""}],""name"":""MintToken"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_from"",""type"":""address""},{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""Transfer"",""outputs"":[{""name"":""success"",""type"":""bool""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""name"":""initialSupply"",""type"":""uint256""},{""name"":""tokenName"",""type"":""string""},{""name"":""tokenSymbol"",""type"":""string""},{""name"":""centralMinter"",""type"":""address""},{""name"":""sellPr"",""type"":""uint256""},{""name"":""buyPr"",""type"":""uint256""}],""payable"":false,""stateMutability"":""nonpayable"",""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""from"",""type"":""address""},{""indexed"":true,""name"":""to"",""type"":""address""},{""indexed"":false,""name"":""value"",""type"":""uint256""}],""name"":""TransferEvent"",""type"":""event""}]";
        readonly string testnetURL = "http://192.168.21.56:52353";
#pragma warning restore IDE0044 // Add readonly modifier

        //Create new account, returns account address
        public string CreateNew(string path, string password)
        {
            Nethereum.Signer.EthECKey ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            string address = ecKey.GetPublicAddress();
            KeyStoreService service = new KeyStoreService();
            string encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address);
            string filename = service.GenerateUTCFileName(address);

            SaveToKeystore(path, filename, encryptedKey);

            return address;
        }
        
        public void SaveToKeystore(string path, string filename, string encryptedKey)
        {
            using (var newFile = File.CreateText(Path.Combine(path, filename)))
            {
                newFile.Write(encryptedKey);
                newFile.Flush();
            }
        }
        //Load account from existing keystore
        public string LoadFromKeystore(string filepath)
        {
            using (var oldFile = File.OpenText(filepath))
            {
                string json = oldFile.ReadToEnd();
                string address = "";
                dynamic keystore = JsonConvert.DeserializeObject(json);
                var temp = keystore.address;
                address = temp.ToString();
                if (address.Length < 42)
                    address = "0x" + address;
                return address;
            }
        }

        //ContractToNethereum function parser
        public Function GetFunction(string senderAddress, string password, string contractFunction)
        {
            ManagedAccount account = new ManagedAccount(senderAddress, password);
            Web3Geth web3 = new Web3Geth(account, testnetURL);
            Contract contract = web3.Eth.GetContract(abi, contractAddress);
            return contract.GetFunction(contractFunction);
        }

        //CheckBalance & GetSellPrice & GetBuyPrice
        public async Task<BigInteger> CallFunction(string senderAddress, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null);
            return await function.CallAsync<BigInteger>(senderAddress, gas, null);
        }
        //Buy & Sell & Donate
        public async Task CallFunction(string senderAddress, BigInteger value, Function function, int soCSharpKnowsDifference)
        {
            //Buy & Donate
            if (soCSharpKnowsDifference == 0)
            {
                HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, new HexBigInteger(value));
                await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, new HexBigInteger(value));
            }
            //Sell
            else if (soCSharpKnowsDifference == 1)
            {
                HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, value);
                await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, value);
            }
        }
        //CheckRequiredFunds
        public async Task<bool> CallFunction(string senderAddress, BigInteger value, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, value);
            return await function.CallAsync<bool>(senderAddress, gas, null, value);
        }
        //PayUp & MintToken & Transfer
        public async Task CallFunction(string senderAddress, string receiverAddress, BigInteger value, Function function, int payupORtransfer = 0)
        {
            if (payupORtransfer == 0)
            {
                HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, receiverAddress, value);
                await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, receiverAddress, value);
            }
            else if (payupORtransfer == 1)
            {
                HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, senderAddress, receiverAddress, value);
                await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, senderAddress, receiverAddress, value);
            }
        }
        //SetPrice
        public async Task CallFunction(string senderAddress, BigInteger sellPrice, BigInteger buyPrice, Function function)
        {
            HexBigInteger gas = await function.EstimateGasAsync(senderAddress, null, null, sellPrice, buyPrice);
            await function.SendTransactionAndWaitForReceiptAsync(senderAddress, gas, null, null, sellPrice, buyPrice);
        }



        public async Task PayUp(string senderAddress, string password, string receiverAddress, BigInteger value)
        {
            Function payUp = GetFunction(senderAddress, password, "PayUp");
            await CallFunction(senderAddress, receiverAddress, value, payUp);
        }
        //Only token creator can use it
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

        public async Task Buy(string senderAddress, string password, BigInteger valueInETH)
        {
            Function buy = GetFunction(senderAddress, password, "Buy");
            await CallFunction(senderAddress, valueInETH, buy, 0);
        }

        public async Task Sell(string senderAddress, string password, BigInteger valueInBDWei)
        {
            Function sell = GetFunction(senderAddress, password, "Sell");
            await CallFunction(senderAddress, valueInBDWei, sell, 1);
        }

        public async Task<BigInteger> GetSellPrice(string senderAddress)
        {
            Function getSellPrice = GetFunction(senderAddress, "", "GetSellPrice");
            return await CallFunction(senderAddress, getSellPrice);
        }

        public async Task<BigInteger> GetBuyPrice(string senderAddress)
        {
            Function getBuyPrice = GetFunction(senderAddress, "", "GetBuyPrice");
            return await CallFunction(senderAddress, getBuyPrice);
        }

        public async Task Transfer(string senderAddress, string password, string receiverAddress, BigInteger value)
        {
            Function transfer = GetFunction(senderAddress, password, "Transfer");
            await CallFunction(senderAddress, password, value, transfer, 1);
        }

        public async Task BloodForTheBloodGod(string senderAddress, string password, BigInteger valueInETH)
        {
            Function skullsForTheSkullThrone = GetFunction(senderAddress, password, "BloodForTheBloodGod");
            await CallFunction(senderAddress, valueInETH, skullsForTheSkullThrone, 0);
        }
    }
}