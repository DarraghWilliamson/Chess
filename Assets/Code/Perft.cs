using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using UnityEngine;
using System.Diagnostics;

public static class Perft {
    static Stopwatch gen, move,undo;
    static string[] lines;
    static GameLogic gameLogic;
    static readonly Dictionary<int, string> promotionRep = new Dictionary<int, string>() {
        [Move.Flag.PromotionBishop] = "b",
        [Move.Flag.PromotionKnight] = "n",
        [Move.Flag.PromotionQueen] = "q",
        [Move.Flag.PromotionRook] = "r",
        [3] = "",
        [2] = "",
        [1] = "",
        [0] = ""
    };
    struct Test {
        public string fen;
        public List<(int, int)> depths;
        public Test(string f) {
            fen = f;
            depths = new List<(int, int)>();
        }
    }

    public static void RunTests() {
        gameLogic = GameLogic.instance;
        gameLogic.show = false;
        List<Test> tests = ReadPerftTests();
        string[][] results = new string[tests.Count][];
        for (int i = 0; i < tests.Count; i++) {
            string[] result = new string[tests[i].depths.Count];
            gameLogic.LoadFen(tests[i].fen);
            for (int d = 0; d < tests[i].depths.Count; d++) {
                string line = "";
                bool con;
                try {
                    uint r = AutoTester(tests[i].depths[d].Item1);
                    con = r == tests[i].depths[d].Item2;
                    line += " : " + r + " ";
                    line += "" + con.ToString();
                } catch (ArgumentException e) {
                    line = "Error: " + e;
                    con = false;
                }
                result[d] = line;
                if (!con) break;
            }
            results[i] = result;
        }
        WritePerftTests(results);
    }

    static void WritePerftTests(string[][] results) {
        string txtPath = Path.Combine(Environment.CurrentDirectory, "PerftTests.txt");
        if (!File.Exists(txtPath)) Console.Write("nopath");
        
        int dep = 0;
        int res = 0;
        for (int l = 0; l < lines.Length; l++) {
            if (lines[l] == null) continue;
            if (lines[l].StartsWith("//")) continue;
            if (lines[l].StartsWith("FEN: ")) { res++; dep = 0; }
            if (lines[l].StartsWith("Depth") && !lines[l].EndsWith("true")) {
                lines[l] += results[res - 1][dep];
                dep++;
            }
        }
        File.WriteAllLines(txtPath, lines); ;
    }

    static List<Test> ReadPerftTests() {
        string txtPath = Path.Combine(Environment.CurrentDirectory, "PerftTests.txt");
        if (!File.Exists(txtPath)) Console.Write("nopath");
        List<Test> tests = new List<Test>();

        lines = File.ReadAllLines(txtPath);
        foreach (string line in lines) {
            if (line == null) continue;
            if (line.StartsWith("//")) continue;
            if (line.StartsWith("FEN: ")) {
                string f = line.Substring(line.IndexOf(' ') + 1);
                tests.Add(new Test(f));
            }
            if (line.StartsWith("Depth") && !line.EndsWith("true")) {
                string[] split = line.Split(new char[] { ' ' });
                int d = int.Parse(split[1]);
                int r = int.Parse(split[2]);
                tests[tests.Count - 1].depths.Add((d, r));
            }
        }
        return tests;
    }
    static uint AutoTester(int ina) {
        gameLogic = GameLogic.instance;
        gameLogic.show = false;
        uint moves = AutoTest(ina);
        return moves;
    }
    static uint AutoTest(int depth) {
        if (depth == 0) return 1;
        List<Move> allMoves = gameLogic.board.GenerateMoves();
        uint numPos = 0;
        foreach (Move m in allMoves) {
            gameLogic.board.MovePiece(m);
            numPos += MoveGenTest(depth - 1);
            gameLogic.board.CtrlZ(m);
        }
        return numPos;
    }

    public static void MoveTestSplit(string fen, int depth) {
        gameLogic = GameLogic.instance;
        //gameLogic.LoadFen(fen);
        gameLogic.show = false;
        StringBuilder sb = new StringBuilder();
        gameLogic = GameLogic.instance;
        uint total = 0;
        gameLogic.show = false;
        var watch = Stopwatch.StartNew();
        List<Move> allMoves = gameLogic.board.GenerateMoves();
        for (int i = 0; i < allMoves.Count; i++) {
            uint moves = MoveGenTestSplt(depth, allMoves[i], gameLogic.board);
            total += moves;
            sb.Append(gameLogic.GetBoardRep(allMoves[i].StartSquare) + gameLogic.GetBoardRep(allMoves[i].EndSquare) + promotionRep[allMoves[i].MoveFlag] + ": " + moves + "\n");
        }
        UnityEngine.Debug.Log(total + " total moves" + " in " + watch.ElapsedMilliseconds + " ms\n" + sb.ToString());
        watch.Stop();
    }
    static uint MoveGenTestSplt(int depth, Move move, Board board) {
        if (depth == 0) return 1;
        uint numPos = 0;
        board.MovePiece(move);
        numPos += MoveGenTest(depth - 1);
        board.CtrlZ(move);
        return numPos;
    }

    


    public static void MoveTester(int depth) {
        gameLogic = GameLogic.instance;
        gameLogic.show = false;
        gen = new Stopwatch();
        move = new Stopwatch();
        undo = new Stopwatch();
        var watch = Stopwatch.StartNew();
        uint moves = MoveGenTest(depth);
        watch.Stop();

        StringBuilder b = new StringBuilder();
        b.Append(moves + "moves, ");
        b.Append(watch.ElapsedMilliseconds + "ms - ");
        b.Append(gen.ElapsedMilliseconds + "moveGen - ");
        b.Append(move.ElapsedMilliseconds + "move - ");
        b.Append(undo.ElapsedMilliseconds + "undo - ");
        UnityEngine.Debug.Log(b.ToString());
    }

    static uint MoveGenTest(int depth) {
        if (depth == 0) return 1;
        gen.Start();
        List<Move> allMoves = gameLogic.board.GenerateMoves();
        gen.Stop();
        uint numPos = 0;
        foreach (Move m in allMoves) {
            move.Start();
            gameLogic.board.MovePiece(m);
            move.Stop();
            numPos += MoveGenTest(depth - 1);
            undo.Start();
            gameLogic.board.CtrlZ(m);
            undo.Stop();
        }
        return numPos;
    }
    public static void MoveTester2(string fen, int depth) {

        gameLogic = GameLogic.instance;
        gameLogic.LoadFen(fen);
        gameLogic.show = false;
        var watch = Stopwatch.StartNew();
        uint moves = MoveGenTest2(depth);
        watch.Stop();
        UnityEngine.Debug.Log(moves + " moves, " + watch.ElapsedMilliseconds + " ms");
    }

    static uint MoveGenTest2(int depth) {
        if (depth == 0) return 1;

        uint numPos = 0;
        numPos += gameLogic.table.GetEnrtyNodes();
        if (numPos == 0) {
            List<Move> allMoves = gameLogic.board.GenerateMoves();
            foreach (Move m in allMoves) {
                gameLogic.board.MovePiece(m);
                numPos += MoveGenTest2(depth - 1);
                gameLogic.board.CtrlZ(m);
            }
            gameLogic.table.StoreEntry(numPos);
        }

        return numPos;
    }




}
