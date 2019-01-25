using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Todo.Models;
using System.Xml.Linq;
using System.Net;
using Windows.Storage;

namespace Todo.Services
{
    public class TileService
    {
        private const string TileTemplateXml = @"
            <tile>
              <visual>

                <binding template='TileSmall' displayName = 'Todo' branding = 'name'>
                  <image src='Assets/optionHeader.jpg' placement='background'/>
                  <text>{0}</text>
                  <text hint-style='captionsubtle' hint-wrap='true'>{1}</text>
                </binding>

                <binding template = 'TileMedium' displayName = 'Todo' branding = 'name'>
                  <image src='Assets/optionHeader.jpg' placement='background'/>
                  <text>{0}</text>
                  <text hint-style='captionsubtle' hint-wrap='true'>{1}</text>
                </binding>

                <binding template = 'TileWide'  displayName = 'Todo' branding = 'name'>
                  <image src='Assets/optionHeader.jpg' placement='background'/>
                  <text>{0}</text>
                  <text hint-style='captionsubtle' hint-wrap='true'>{1}</text>
                </binding>

                <binding template = 'TileLarge'  displayName = 'Todo' branding = 'name'>
                   <image src='Assets/optionHeader.jpg' placement='background'/>
                  <text>{0}</text>
                  <text hint-style='captionsubtle' hint-wrap='true'>{1}</text>
                </binding>

              </visual>
            </tile>";

        static public void SetBadgeCountOnTile(int count)
        {
            // Update the badge on the real tile
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());

            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        public static Windows.Data.Xml.Dom.XmlDocument CreateTiles(PrimaryTile primaryTile)
        {
            XDocument xDoc = new XDocument(
                new XElement("tile", new XAttribute("version", 3),
                    new XElement("visual",
                        // Small Tile
                        new XElement("binding", new XAttribute("branding", primaryTile.branding), new XAttribute("displayName", primaryTile.appName), new XAttribute("template", "TileSmall"),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", primaryTile.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", primaryTile.description, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),

                        // Medium Tile
                        new XElement("binding", new XAttribute("branding", primaryTile.branding), new XAttribute("displayName", primaryTile.appName), new XAttribute("template", "TileMedium"),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", primaryTile.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", primaryTile.description, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),

                        // Large Tile
                        new XElement("binding", new XAttribute("branding", primaryTile.branding), new XAttribute("displayName", primaryTile.appName), new XAttribute("template", "TileLarge"),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", primaryTile.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", primaryTile.description, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),

                        // Wide Tile
                        new XElement("binding", new XAttribute("branding", primaryTile.branding), new XAttribute("displayName", primaryTile.appName), new XAttribute("template", "TileWide"),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", primaryTile.title, new XAttribute("hint-style", "caption")),
                                    new XElement("text", primaryTile.description, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        )
                    )
                )
            );

            Windows.Data.Xml.Dom.XmlDocument xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }

        public static void UpdatePrimaryTileByFormat()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            List<TodoItem> items = MainPage.latestInstance.TileList();
            if (items == null || !items.Any())
            {
                return;
            }

            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);
                updater.Clear();

                foreach (var n in items)
                {
                    var doc = new XmlDocument();
                    var xml = string.Format(TileTemplateXml, n.Title, n.Description);
                    doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
                    {
                        ProhibitDtd = false,
                        ValidateOnParse = false,
                        ElementContentWhiteSpace = false,
                        ResolveExternals = false
                    });
                    updater.Update(new TileNotification(doc));
                }
            }
            catch (Exception ee)
            {
                EditItem.latestInstance.showMessageDialog(ee.ToString());
            }
        }

        public static void UpdatePrimaryTileByCode()
        {

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            List<TodoItem> items = MainPage.latestInstance.TileList();
            foreach (var n in items)
            {
                var xmlDoc = TileService.CreateTiles(new PrimaryTile(n.Title, n.Description));
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Update(new TileNotification(xmlDoc));
            }
        }
    }
}
