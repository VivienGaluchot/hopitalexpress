using System.Collections.Generic;


public class Player {
    
    public static readonly List<Player> All = new List<Player>();

    public struct SkinData {
        public int headId;
        public int skinId;
        public int clothesId;
    }


    public readonly PlayerInput input;

    public readonly SkinData skin;

    public Player(PlayerInput input, SkinData skin) {
        this.input = input;
        this.skin = skin;
    }

}
