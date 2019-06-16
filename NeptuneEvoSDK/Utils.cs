using GTANetworkAPI;

namespace Redage.SDK
{
    public class Utils
    {
        public static AccountData GetAccount(Client client)
        {
            //client.GetExternalData<AccountData>(0);
            //return new AccountData();
            return client.GetData("AccData");
        }
        public static CharacterData GetCharacter(Client client)
        {
            //client.GetExternalData<CharacterData>(1);
            //return new CharacterData();
            return client.GetData("CharData");
        }
    }
}
