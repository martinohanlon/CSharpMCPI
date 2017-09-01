using System;

namespace MinecraftPiAPI
{
    class Test
    {
        public static void Main()
        {
            // connect and post to chat
            Minecraft minecraft = new Minecraft();
            minecraft.Chat.Post("hello world");

            // use players position
            minecraft.Player.Position.X += 10;
            minecraft.Player.Position.Y += 10;
            minecraft.Player.Position.Z += 10;

            // Get and Set the players position
            Position playersPosition = minecraft.Player.Position.Get();
            playersPosition.Y += 1;
            minecraft.Player.Position.Set(playersPosition);
            Double x = playersPosition.X; 
            Double y = playersPosition.Y; 
            Double z = playersPosition.Z;
            minecraft.Player.Position.Set(x, y, z);

            // create a new position
            y = y - 1;
            Position belowPlayer = new Position(x, y, z);

            // use the world
            Int32 height = minecraft.World.Height(x, z);
            minecraft.Chat.Post(String.Format("the height of the world is {0}", height));

            // update blocks
            minecraft.World.Block(x,y,z).Id = 35;
            minecraft.World.Block(x,y,z).Data = 10;
            minecraft.World.Block(belowPlayer).Id = Blocks.Stone.Id;
            minecraft.World.Block(minecraft.Player.Position).Set(Blocks.TntActive);

            // mixing types
            Position playerPos = minecraft.Player.Position.Get();
            Block myBlock = minecraft.World.Block(playerPos);
            Block blockBelow = minecraft.World.Block(myBlock.X, myBlock.Y - 1, myBlock.Z);
            blockBelow.Set(2);

        }
    }
}