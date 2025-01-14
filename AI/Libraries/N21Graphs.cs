﻿namespace AI.Libraries;

public static class N21Graphs
{
    // 2-2-1
    public static (int i, int j)[][] Arrow => [[(0, 2), (0, 3), (1, 2), (1, 3)], [(2, 4), (3, 4)]];

    // 2-3-1
    public static (int i, int j)[][] Fly => [[(0, 2), (0, 3), (0, 4), (1, 2), (1, 3), (1, 4)], [(2, 5), (3, 5), (4, 5)]];

    // 2-4-1
    public static (int i, int j)[][] Moon => [[(0, 2), (0, 3), (0, 4), (1, 3), (1, 4), (1, 5)], [(2, 6), (3, 6), (4, 6), (5, 6)]];
    public static (int i, int j)[][] Moon1 => [[(0, 2), (0, 4), (0, 5), (1, 3), (1, 5), (1, 4)], [(2, 6), (3, 6), (4, 6), (5, 6)]];
    public static (int i, int j)[][] Moon2 => [[(0, 2), (0, 3), (0, 5), (1, 3), (1, 5), (1, 4)], [(2, 6), (3, 6), (4, 6), (5, 6)]];
    public static (int i, int j)[][] StrangeMoon => [[(0, 2), (0, 3), (0, 4), (0, 6), (1, 3), (1, 4), (1, 5), (1, 6)], [(2, 6), (3, 6), (4, 6), (5, 6)]];

    // 2-6-1
    public static (int i, int j)[][] MercuryShadow => [[(0, 2), (0, 4), (0, 6), (0, 7), (1, 3), (1, 5), (1, 7), (1, 2)], [(2, 8), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8)]];
    public static (int i, int j)[][] Mercury => [[(0, 2), (0, 4), (0, 6), (0, 5), (0, 7), (1, 3), (1, 2), (1, 4), (1, 5), (1, 7)], [(2, 8), (3, 8), (4, 8), (5, 8), (6, 8), (7, 8)]];

    // 2-7-1
    public static (int i, int j)[][] Mars => [[(0, 2), (0, 3), (0, 5), (0, 6), (0, 7), (0, 8), (1, 2), (1, 3), (1, 4), (1, 5), (1, 7), (1, 8)], [(2, 9), (3, 9), (4, 9), (5, 9), (6, 9), (7, 9), (8, 9)]];

    // 2-9-1
    public static (int i, int j)[][] Venus => [[(0, 2), (0, 3), (0, 4), (0, 5), (0, 6), (0, 7), (0, 9), (0, 10), (1, 2), (1, 3), (1, 5), (1, 7), (1, 8), (1, 9), (1, 10)], [(2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11)]];
    public static (int i, int j)[][] StrangeVenus => [[(0, 2), (0, 3), (0, 4), (0, 5), (0, 6), (0, 7), (0, 9), (0, 10), (0, 11), (1, 2), (1, 3), (1, 5), (1, 7), (1, 8), (1, 9), (1, 10), (1, 11)], [(2, 11), (3, 11), (4, 11), (5, 11), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11)]];

    public static (int i, int j)[][] Atlant =>
    [
        [(0, 2), (0, 4), (0, 6), (0, 8), (0, 3), (0, 5), (1, 3), (1, 5), (1, 7), (1, 9)],
        [(2, 10), (2, 12), (3, 11), (3, 13), (3, 16), (4, 12), (4, 10), (4, 13), (5, 13), (6, 14), (6, 10), (6, 13), (7, 15), (8, 16), (9, 17), (9, 15), (9, 13)],
        [(10, 18), (11, 18), (12, 18), (13, 18), (14, 18), (15, 18), (16, 18), (17, 18)]
    ];

    public static (int i, int j)[][] StrangeAtlant =>
    [
        [(0, 2), (0, 4), (0, 6), (0, 8), (0, 3), (0, 5), (0, 18), (1, 3), (1, 5), (1, 7), (1, 9), (1, 18)],
        [(2, 10), (2, 12), (2, 18), (3, 11), (3, 13), (3, 16), (4, 12), (4, 10), (4, 13), (5, 13), (6, 14), (6, 10), (6, 13), (7, 15), (7, 18), (8, 16), (9, 17), (9, 15), (9, 13), (9, 18)],
        [(10, 18), (11, 18), (12, 18), (13, 18), (14, 18), (15, 18), (16, 18), (17, 18)]
    ];

    // 2-19-1
    public static (int i, int j)[][] Xanthippe = [[(0, 2), (0, 4), (0, 6), (0, 7), (0, 8), (0, 10), (0, 12), (0, 13), (0, 14), (0, 16), (0, 18), (0, 20), (1, 3), (1, 5), (1, 7), (1, 8), (1, 9), (1, 11), (1, 13), (1, 14), (1, 15), (1, 17), (1, 19)], [(2, 21), (3, 21), (4, 21), (5, 21), (6, 21), (7, 21), (8, 21), (9, 21), (10, 21), (11, 21), (12, 21), (13, 21), (14, 21), (15, 21), (16, 21), (17, 21), (18, 21), (19, 21), (20, 21)]];

    // 2-19-19-1
    public static (int i, int j)[][] Socrates => [[(0, 2), (0, 4), (0, 6), (0, 7), (0, 8), (0, 10), (0, 12), (0, 13), (0, 14), (0, 16), (0, 18), (0, 20), (1, 3), (1, 5), (1, 7), (1, 8), (1, 9), (1, 11), (1, 13), (1, 14), (1, 15), (1, 17), (1, 19)], [(2, 21), (2, 23), (2, 26), (3, 22), (3, 25), (3, 30), (3, 32), (3, 33), (3, 37), (4, 23), (4, 26), (4, 28), (4, 30), (4, 37), (4, 39), (5, 24), (5, 36), (5, 38), (6, 25), (6, 36), (7, 26), (7, 31), (7, 33), (7, 36), (8, 25), (8, 26), (8, 27), (9, 28), (9, 31), (10, 21), (10, 22), (10, 29), (10, 34), (10, 35), (10, 37), (11, 23), (11, 26), (11, 29), (11, 30), (12, 23), (12, 25), (12, 27), (12, 29), (12, 31), (13, 21), (13, 29), (13, 30), (13, 32), (13, 34), (14, 30), (14, 32), (14, 33), (14, 35), (14, 37), (15, 25), (15, 26), (15, 33), (15, 34), (15, 36), (15, 37), (16, 23), (16, 29), (16, 33), (16, 35), (17, 21), (17, 23), (17, 24), (17, 29), (17, 36), (18, 21), (18, 22), (18, 26), (18, 27), (18, 36), (18, 37), (19, 22), (19, 24), (19, 25), (19, 27), (19, 33), (19, 35), (19, 36), (19, 37), (19, 38), (20, 22), (20, 30), (20, 34), (20, 39)], [(21, 40), (22, 40), (23, 40), (24, 40), (25, 40), (26, 40), (27, 40), (28, 40), (29, 40), (30, 40), (31, 40), (32, 40), (33, 40), (34, 40), (35, 40), (36, 40), (37, 40), (38, 40), (39, 40)]];

    public static (int i, int j)[][] TreeOnMercury => [[(0, 2), (0, 4), (0, 5), (0, 6), (0, 7), (1, 2), (1, 3), (1, 4), (1, 5), (1, 7)], [(2, 8), (2, 10), (2, 12), (3, 8), (3, 9), (3, 10), (3, 11), (4, 8), (4, 9), (4, 10), (5, 9), (5, 11), (5, 12), (6, 8), (6, 11), (6, 12), (7, 8), (7, 9), (7, 11)], [(8, 13), (8, 14), (8, 16), (9, 14), (9, 15), (9, 16), (10, 15), (10, 16), (11, 13), (11, 14), (11, 15), (11, 16), (12, 14)], [(13, 17), (13, 18), (14, 17), (14, 18), (15, 18), (15, 19), (16, 17), (16, 18), (16, 19)], [(17, 20), (18, 20), (19, 20)]];
}
