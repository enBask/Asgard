using Asgard.Core.Network.Data;
using MonoGame.Extended.Maps.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asgard.Core.System;
using Artemis;
using System.Reflection;
using Farseer.Framework;
using Asgard.Core.Physics;
using FarseerPhysics.Factories;

namespace Shared
{
    public class MapData : DefinitionNetworkObject
    {
        public NetworkProperty<int> Width { get; set; }
        public NetworkProperty<int> Height { get; set; }
        public NetworkProperty<int> TileWidth { get; set; }
        public NetworkProperty<int> TileHeight { get; set; }
        public NetworkProperty<int> FirstId { get; set; }

        public NetworkProperty<LayerData> Layer1 { get; set; }
        public NetworkProperty<LayerData> Layer2 { get; set; }
        public NetworkProperty<LayerData> Layer3 { get; set; }
        public NetworkProperty<LayerData> Layer4 { get; set; }
        public NetworkProperty<LayerData> Layer5 { get; set; }

        float _worldfactor = 1f / 10f;
        public MapData()
        {

        }

        public void Load(AsgardBase Base, TiledMap map)
        {
            Width = map.Width;
            Height = map.Height;
            TileWidth = map.TileWidth;
            TileHeight = map.TileHeight;
            FirstId = 1;

            var layers = map.Layers.ToList();
            if (layers.Count < 5) return;

            Layer1 = new LayerData(layers[0] as TiledTileLayer);
            Layer2 = new LayerData(layers[1] as TiledTileLayer);
            Layer3 = new LayerData(layers[2] as TiledTileLayer);
            Layer4 = new LayerData(layers[3] as TiledTileLayer);
            Layer5 = new LayerData(layers[4] as TiledTileLayer);

            LoadCollisions(Base, map);

         
        }

        private void LoadCollisions(AsgardBase Base, TiledMap map)
        {
            #region build collision world
            var viewMap = map;
            var midgard = Base.LookupSystem<Midgard>();
            var world = midgard.GetWorld();

            var layer = viewMap.Layers.Last() as TiledTileLayer;
            {
                var layerField = typeof(TiledMap).GetField("_layers", BindingFlags.NonPublic | BindingFlags.Instance);

                List<TiledLayer> layers = layerField.GetValue(viewMap) as List<TiledLayer>;
                layers.Remove(layer);
            }

            for (var y = 0; y < layer.Height; y++)
            {
                for (var x = 0; x < layer.Width; x++)
                {
                    var tile = layer.GetTile(x, y);

                    if (tile.Id == 1702)// hack
                    {
                        var xTile = tile.X;
                        var yTile = tile.Y;

                        Vector2 centerPoint = new Vector2((xTile * (viewMap.TileWidth - 1)) + ((viewMap.TileWidth - 1) / 2),
                            (yTile * (viewMap.TileHeight - 1)) + ((viewMap.TileHeight - 1) / 2));

                        Vector2 upperLeftPos = new Vector2(xTile * (viewMap.TileWidth - 1),
                            (yTile) * (viewMap.TileHeight - 1));


                        var body = BodyFactory.CreateRectangle(world,
                            (viewMap.TileWidth * _worldfactor) - 0.01f,
                            (viewMap.TileHeight * _worldfactor) - 0.01f,
                            1.0f);

                        body.Restitution = 1f;
                        body.Position = new Vector2(
                            centerPoint.X * _worldfactor,
                            centerPoint.Y * _worldfactor);
                        body.CollisionCategories = FarseerPhysics.Dynamics.Category.Cat1;
                        body.CollidesWith = FarseerPhysics.Dynamics.Category.Cat2;
                    }
                }
            }
            #endregion
        }


        public override void OnCreated(AsgardBase instance, Entity entity)
        {
            var mapComponent = entity.GetComponent<MapComponent>();
            mapComponent.Map = new TiledMap(mapComponent.Device, Width, Height, TileWidth, TileHeight);
            mapComponent.Map.CreateTileset(mapComponent.Texture, FirstId, TileWidth, TileHeight, 1, 1);

            mapComponent.Map.CreateTileLayer("1", Layer1.Value.Width, Layer1.Value.Height, Layer1.Value.TileData);
            mapComponent.Map.CreateTileLayer("2", Layer2.Value.Width, Layer2.Value.Height, Layer2.Value.TileData);
            mapComponent.Map.CreateTileLayer("3", Layer3.Value.Width, Layer3.Value.Height, Layer3.Value.TileData);
            mapComponent.Map.CreateTileLayer("4", Layer4.Value.Width, Layer4.Value.Height, Layer4.Value.TileData);
            mapComponent.Map.CreateTileLayer("5", Layer5.Value.Width, Layer5.Value.Height, Layer5.Value.TileData);

            LoadCollisions(instance, mapComponent.Map);

        }
    }

    public class LayerData : DefinitionNetworkObject
    {
        public NetworkProperty<string> StringData { get; set; }
        public NetworkProperty<int> Height { get; set; }
        public NetworkProperty<int> Width { get; set; }

        public int[] TileData { get; set; }

        public LayerData()
        {
        }

        public LayerData(TiledTileLayer layer)
        {
            Width = layer.Width;
            Height = layer.Height;
            List<int> ids = new List<int>();
            for (int y = 0; y < layer.Height; ++y)
            {
                for (int x = 0; x < layer.Width; ++x)
                {
                    var tile = layer.GetTile(x, y);
                    ids.Add(tile.Id);
                }
            }

            StringData = String.Join(",", ids.ToArray());
        }

        public override void OnCreated(AsgardBase instance, Entity entity)
        {
            var strData = StringData.Value.Split(',');
            TileData = strData.Select(s => Convert.ToInt32(s)).ToArray();
        }
    }
}
