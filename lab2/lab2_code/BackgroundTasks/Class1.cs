using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class Class1 : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            // TODO: 获取数据，更新磁贴逻辑
            await UpdateTileItems();
            deferral.Complete();
        }
        private IAsyncOperation<string> UpdateTileItems()
        {
            
            try
            {
                //return AsyncInfo.Run(){ token => GetItems() };
                //Todo.TileService.UpdatePrimaryTile();
                
            }
            catch (Exception)
            {
                
            }
            
            return null;
        }
        /*
        //获取信息列表
        private async Task<string> GetItems()
        {

        }
        */
    }
}
