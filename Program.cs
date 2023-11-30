// See https://aka.ms/new-console-template for more information
using Lionpaw;
public static class Program{

    public static void Main(string [] args){
        Lionpaw.Lionpaw lionpaw = new Lionpaw.Lionpaw();
        lionpaw.RunLionpaw().GetAwaiter().GetResult();
    }

}
