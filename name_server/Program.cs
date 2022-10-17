using name_server;


try
{
    Server server = new Server();
    server.Start();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
