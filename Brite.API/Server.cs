namespace Brite.API
{
    public class Server
    {
        /*
        Server runs on a standard port
        
        Client::Client(IPEndPoint remoteServer, string identifier)
        Client::ConnectAsync()

        Client::RequestControllerAsync(Priority priority)
	        - If priority is lower than the currently used controller (by server, the server will respond when the controller is released, or when request timeout is reached, whichever comes first)
	        - If priority is higher than the currently used controller (by server, the server will replace the currently used controller and will notify the old controller that it has been released)
	
        Client::ReleaseControllerAsync()

        -API
         -Server
         -Client
         -Command

        -Service
         -Creates Server and interfaces with Controller
         -Server per Controller?
        */

        // Do we need controller or can we make a server with a direct device?
        // Server -> Device, manages and all that
        // Client -> Server
        // Daemon -> Runs Server
    }
}
