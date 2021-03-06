using Datory;

namespace SiteServer.CMS.Model
{
    [Table("siteserver_Plugin")]
    public class PluginInfo : Entity
    {
        [TableColumn]
        public string PluginId { get; set; }

        [TableColumn]
        private string IsDisabled { get; set; }

        public bool Disabled
        {
            get => IsDisabled == "True";
            set => IsDisabled = value.ToString();
        }

        [TableColumn]
        public int Taxis { get; set; }
    }
}
