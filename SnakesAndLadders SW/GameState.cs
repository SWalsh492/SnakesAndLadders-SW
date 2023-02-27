using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakesAndLadders_SW;

public class GameState
{
	// positions, direction, edges, current player
    // to save the game state, get all the information, create the object
    // and then write to a file.
    public int CurrentPlayer { get; set; }
    public int[] PlayerRows;
    public int[] PLayerColumns;
    public int[] Directions;
    public int[] Edges;

    public GameState()
    {
        PlayerRows = new int[3];
        PLayerColumns = new int[3];
        Directions = new int[3];
        Edges = new int[3];
    }
}
