namespace El_buen_sabor.Components.Models
{
    public class KitchenQueueSnapshotDto
    {
        public List<KitchenQueueItemDto> Cooking { get; set; } = [];
        public List<KitchenQueueItemDto> Waiting { get; set; } = [];
    }
}
