namespace NeuroMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Rejestracja routingu dla nowych stron
            Routing.RegisterRoute("LootBoxPage", typeof(Views.LootBoxPage));
        }
    }
}
