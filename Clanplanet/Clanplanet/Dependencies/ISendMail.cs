

namespace Clanplanet.Dependencies
{
    public interface ISendMail
    {
        void Send(string errorMessage, string jsonData);
    }
}
