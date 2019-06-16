using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEvo.GUI;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.Jobs
{
    class Bus : Script
    {
        private static List<int> BuswaysPayments = new List<int>()
        {
            3, 4, 3, 4, 4, 8
        };
        private static nLog Log = new nLog("Bus");
        
        private static int BusRentCost = 150;
        private static List<String> BusWaysNames = new List<String>
        {
            "ЛСПД  - Центральная Площадь - ФБР",
            "Западное Гетто - Аэропорт -  Центральная Площадь",
            "Западное Гетто - Автошкола - Центральная Площадь",
            "Западное Гетто  - Аэропорт - Центральная Площадь - Газонокосилка - Электростанция - Таксопарк",
            "Западное Гетто - Дальнобойщик - Восточное Гетто -  Инкасатор -  Механик",
            "Центральная Площадь - Чумаш - Армия - Палето Бэй - Сэнди Шорс",
        };
        private static List<List<BusCheck>> BusWays = new List<List<BusCheck>>()
        {
            new List<BusCheck>() // busway1
            {
                new BusCheck(new Vector3(450.9898, -582.5137, 27.37624), false), // 1 
                new BusCheck(new Vector3(421.5236, -623.957, 27.37671), false), // 2 
                new BusCheck(new Vector3(421.6955, -663.074, 27.68965), false), // 3 
                new BusCheck(new Vector3(395.7946, -674.9282, 28.13092), false), // 4 
                new BusCheck(new Vector3(342.5821, -684.7997, 28.22467), false), // 5 
                new BusCheck(new Vector3(299.447, -826.13, 28.21097), false), // 6 
                new BusCheck(new Vector3(313.1335, -865.0632, 28.15827), false), // 7 
                new BusCheck(new Vector3(385.7656, -865.0663, 28.13842), false), // 8 
                new BusCheck(new Vector3(403.3033, -886.5486, 28.28123), false), // 9 
                new BusCheck(new Vector3(394.7838, -992.9272, 28.12644), true), // 10 
                new BusCheck(new Vector3(395.6033, -1019.798, 28.2503), false), // 11 
                new BusCheck(new Vector3(371.0182, -1037.675, 28.11036), false), // 12 
                new BusCheck(new Vector3(255.8246, -1043.63, 28.19174), false), // 13 
                new BusCheck(new Vector3(197.4691, -1025.473, 28.23371), false), // 14 
                new BusCheck(new Vector3(135.5417, -1004.655, 28.28214), false), // 15 
                new BusCheck(new Vector3(78.69234, -983.7912, 28.28188), false), // 16 
                new BusCheck(new Vector3(-2.363072, -954.281, 28.28399), false), // 17 
                new BusCheck(new Vector3(-64.35108, -931.5195, 28.23721), false), // 18 
                new BusCheck(new Vector3(-153.9692, -898.4047, 28.23442), false), // 19 
                new BusCheck(new Vector3(-211.4247, -877.7959, 28.43569), false), // 20 
                new BusCheck(new Vector3(-267.1643, -853.4876, 30.4005), false), // 21
                new BusCheck(new Vector3(-302.2533, -843.1558, 30.53176), false), // 26 
                new BusCheck(new Vector3(-471.5434, -829.6063, 29.32062), false), // 27 
                new BusCheck(new Vector3(-492.6831, -811.0066, 29.34468), false), // 28 
                new BusCheck(new Vector3(-535.6282, -726.0069, 31.8856), false), // 29 
                new BusCheck(new Vector3(-545.4334, -684.6165, 32.17327), false), // 30 
                new BusCheck(new Vector3(-568.0728, -654.0825, 32.09759), false), // 31 
                new BusCheck(new Vector3(-607.026, -649.9659, 30.59394), false), // 32 
                new BusCheck(new Vector3(-624.3636, -631.0801, 30.98599), false), // 33 
                new BusCheck(new Vector3(-624.2582, -572.2528, 33.77871), false), // 34 
                new BusCheck(new Vector3(-623.7788, -509.9615, 33.61566), false), // 35 
                new BusCheck(new Vector3(-622.9012, -445.4083, 33.61018), false), // 36 
                new BusCheck(new Vector3(-621.3241, -396.3221, 33.60853), false), // 37 
                new BusCheck(new Vector3(-593.5432, -377.157, 33.70667), false), // 38 
                new BusCheck(new Vector3(-547.8483, -372.3718, 34.0122), false), // 39 
                new BusCheck(new Vector3(-528.6447, -350.697, 33.99179), false), // 40 
                new BusCheck(new Vector3(-528.8826, -326.8571, 33.90429), true), // 41 
                new BusCheck(new Vector3(-547.5201, -312.0813, 33.97951), false), // 42 
                new BusCheck(new Vector3(-573.4658, -301.9568, 34.04053), false), // 43 
                new BusCheck(new Vector3(-610.5992, -323.1169, 33.7443), false), // 44 
                new BusCheck(new Vector3(-675.3613, -352.3845, 33.50461), false), // 45 
                new BusCheck(new Vector3(-773.1182, -320.4239, 35.67091), false), // 46 
                new BusCheck(new Vector3(-862.8736, -283.3678, 39.22463), false), // 47 
                new BusCheck(new Vector3(-914.614, -291.5288, 38.67406), false), // 48 
                new BusCheck(new Vector3(-987.8404, -329.5891, 36.69654), false), // 49 
                new BusCheck(new Vector3(-1040.111, -355.5928, 36.60289), false), // 50 
                new BusCheck(new Vector3(-1083.602, -379.1826, 35.7047), false), // 51 
                new BusCheck(new Vector3(-1220.133, -422.597, 32.44935), false), // 52 
                new BusCheck(new Vector3(-1281.706, -476.0788, 32.16618), false), // 53 
                new BusCheck(new Vector3(-1360.209, -537.6434, 29.26508), false), // 54 
                new BusCheck(new Vector3(-1390.116, -532.3562, 29.63569), false), // 55 
                new BusCheck(new Vector3(-1444.88, -462.7517, 33.94712), false), // 56 
                new BusCheck(new Vector3(-1489.425, -451.846, 34.49963), false), // 57 
                new BusCheck(new Vector3(-1554.387, -492.2787, 34.51026), false), // 58 
                new BusCheck(new Vector3(-1621.178, -533.2477, 33.2635), true), // 59 
                new BusCheck(new Vector3(-1638.948, -560.8477, 32.35109), false), // 60 
                new BusCheck(new Vector3(-1630.944, -603.2023, 32.05082), false), // 61 
                new BusCheck(new Vector3(-1562.727, -657.8669, 28.03526), false), // 62 
                new BusCheck(new Vector3(-1465.384, -738.5282, 23.35341), false), // 63 
                new BusCheck(new Vector3(-1403.321, -789.3511, 19.09768), false), // 64 
                new BusCheck(new Vector3(-1349.687, -835.5909, 15.95973), false), // 65 
                new BusCheck(new Vector3(-1301.756, -886.1259, 10.61624), false), // 66 
                new BusCheck(new Vector3(-1258.787, -905.8503, 10.23881), false), // 67 
                new BusCheck(new Vector3(-1189.036, -859.813, 12.91326), false), // 68 
                new BusCheck(new Vector3(-1096.284, -782.6707, 18.1074), false), // 69 
                new BusCheck(new Vector3(-880.3716, -665.7205, 26.67558), false), // 70 
                new BusCheck(new Vector3(-769.0373, -666.1133, 28.81329), false), // 71 
                new BusCheck(new Vector3(-663.4095, -667.3198, 30.34066), false), // 72 
                new BusCheck(new Vector3(-569.3216, -667.1436, 31.9665), false), // 73 
                new BusCheck(new Vector3(-464.8506, -666.3396, 31.19046), false), // 74 
                new BusCheck(new Vector3(-288.061, -667.8103, 32.04972), false), // 75 
                new BusCheck(new Vector3(-151.8947, -713.9205, 33.57567), false), // 76 
                new BusCheck(new Vector3(6.096367, -768.9876, 30.66755), false), // 77 
                new BusCheck(new Vector3(55.27623, -787.2473, 30.58677), false), // 78 
                new BusCheck(new Vector3(143.6654, -818.741, 30.008), false), // 79 
                new BusCheck(new Vector3(198.4921, -833.7538, 29.82177), false), // 80 
                new BusCheck(new Vector3(262.5556, -850.8363, 28.28457), false), // 81 
                new BusCheck(new Vector3(304.1494, -828.8317, 28.20259), false), // 82 
                new BusCheck(new Vector3(325.8743, -782.3608, 28.09294), false), // 83 
                new BusCheck(new Vector3(357.3802, -691.244, 28.1174), false), // 84 
                new BusCheck(new Vector3(384.1356, -679.3948, 28.11889), false), // 85 
                new BusCheck(new Vector3(437.084, -680.1784, 27.78469), false), // 86 
                new BusCheck(new Vector3(465.0021, -652.0553, 26.87551), false), // 87 
                new BusCheck(new Vector3(468.9034, -619.5801, 27.37402), false), // 88 
            }, 
            new List<BusCheck>() // busway2
            {
                new BusCheck(new Vector3(447.687, -586.2853, 27.37678), false), // 1 
                new BusCheck(new Vector3(421.6629, -626.4494, 27.37682), false), // 2 
                new BusCheck(new Vector3(421.7111, -661.3333, 27.62286), false), // 3 
                new BusCheck(new Vector3(385.6422, -672.5781, 28.11092), false), // 4 
                new BusCheck(new Vector3(346.033, -690.8776, 28.22015), false), // 5 
                new BusCheck(new Vector3(293.9677, -826.3942, 28.21164), false), // 6 
                new BusCheck(new Vector3(256.8758, -927.3657, 28.08798), false), // 7 
                new BusCheck(new Vector3(225.999, -1011.92, 28.19136), false), // 8 
                new BusCheck(new Vector3(205.7679, -1112.196, 28.21853), false), // 9 
                new BusCheck(new Vector3(210.0018, -1263.712, 28.12389), false), // 10 
                new BusCheck(new Vector3(166.8684, -1369.549, 28.22103), false), // 11 
                new BusCheck(new Vector3(127.6678, -1416.443, 28.21777), false), // 12 
                new BusCheck(new Vector3(75.63905, -1487.545, 28.21854), false), // 13 
                new BusCheck(new Vector3(21.07578, -1533.035, 28.06069), true), // 14 
                new BusCheck(new Vector3(-10.62357, -1581.169, 28.21802), false), // 15 
                new BusCheck(new Vector3(-64.00823, -1636.008, 28.12891), false), // 16 
                new BusCheck(new Vector3(-125.9649, -1711.04, 28.60886), false), // 17 
                new BusCheck(new Vector3(-185.6785, -1773.997, 28.62974), false), // 18 
                new BusCheck(new Vector3(-239.2137, -1806.291, 28.54486), false), // 19 
                new BusCheck(new Vector3(-357.4284, -1820.277, 21.74705), false), // 20 
                new BusCheck(new Vector3(-413.3906, -1841.433, 19.29451), false), // 21 
                new BusCheck(new Vector3(-501.4429, -1908.532, 16.21416), false), // 22 
                new BusCheck(new Vector3(-677.2667, -2087.702, 13.59647), false), // 23 
                new BusCheck(new Vector3(-766.3788, -2184.752, 14.49905), false), // 24 
                new BusCheck(new Vector3(-841.8875, -2264.178, 15.56709), false), // 25 
                new BusCheck(new Vector3(-884.3683, -2341.325, 13.75766), false), // 26 
                new BusCheck(new Vector3(-958.6754, -2409.462, 12.64145), false), // 27 
                new BusCheck(new Vector3(-998.5095, -2454.484, 12.58298), false), // 29 
                new BusCheck(new Vector3(-1040.008, -2526.389, 12.57349), false), // 30 
                new BusCheck(new Vector3(-1075.07, -2585.924, 12.54564), false), // 31 
                new BusCheck(new Vector3(-1093.676, -2641.118, 12.6091), false), // 32 
                new BusCheck(new Vector3(-1079.215, -2688.098, 12.58512), false), // 33 
                new BusCheck(new Vector3(-1031.589, -2724.396, 12.54866), true), // 34 
                new BusCheck(new Vector3(-984.0586, -2744.608, 12.60651), false), // 35 
                new BusCheck(new Vector3(-913.2713, -2718.177, 12.65575), false), // 36 
                new BusCheck(new Vector3(-877.3538, -2640.499, 12.69418), false), // 37 
                new BusCheck(new Vector3(-842.9944, -2578.653, 12.69038), false), // 38 
                new BusCheck(new Vector3(-806.3817, -2540.937, 12.58854), false), // 39 
                new BusCheck(new Vector3(-752.2061, -2429.237, 13.4341), false), // 40 
                new BusCheck(new Vector3(-738.4274, -2371.819, 13.71631), false), // 41 
                new BusCheck(new Vector3(-790.0333, -2319.421, 13.5263), false), // 42 
                new BusCheck(new Vector3(-868.9871, -2240.147, 5.139782), false), // 43 
                new BusCheck(new Vector3(-886.6941, -2188.864, 7.397064), true), // 44 
                new BusCheck(new Vector3(-917.9137, -2178.241, 7.693004), false), // 45 
                new BusCheck(new Vector3(-948.5641, -2159.812, 7.765808), false), // 46 
                new BusCheck(new Vector3(-1034.324, -2074.575, 12.35458), false), // 47 
                new BusCheck(new Vector3(-1088.264, -1957.321, 12.03569), false), // 48 
                new BusCheck(new Vector3(-984.6737, -1851.278, 17.5268), false), // 49 
                new BusCheck(new Vector3(-857.6578, -1722.241, 17.84307), false), // 50 
                new BusCheck(new Vector3(-767.1343, -1613.34, 13.40275), false), // 51 
                new BusCheck(new Vector3(-707.0054, -1528.505, 11.7417), false), // 52 
                new BusCheck(new Vector3(-676.7062, -1471.93, 9.440069), false), // 53 
                new BusCheck(new Vector3(-647.4733, -1431.671, 9.530546), false), // 54 
                new BusCheck(new Vector3(-630.8387, -1325.521, 9.428109), false), // 55 
                new BusCheck(new Vector3(-546.7005, -1173.879, 17.70358), false), // 56 
                new BusCheck(new Vector3(-533.377, -1109.852, 21.00768), false), // 57 
                new BusCheck(new Vector3(-539.2562, -984.7713, 22.24456), false), // 58 
                new BusCheck(new Vector3(-539.2547, -984.7462, 22.24428), false), // 59 
                new BusCheck(new Vector3(-574.5814, -956.3324, 22.04509), false), // 60 
                new BusCheck(new Vector3(-630.5449, -923.4929, 22.2623), false), // 61 
                new BusCheck(new Vector3(-630.2236, -865.1041, 23.75171), false), // 62 
                new BusCheck(new Vector3(-629.436, -686.7356, 29.90144), false), // 63 
                new BusCheck(new Vector3(-624.3752, -575.9335, 33.67994), false), // 63 
                new BusCheck(new Vector3(-624.3804, -575.9319, 33.67971), false), // 64 
                new BusCheck(new Vector3(-623.135, -494.4057, 33.61842), false), // 65 
                new BusCheck(new Vector3(-620.8865, -398.0507, 33.59068), false), // 66 
                new BusCheck(new Vector3(-597.3029, -377.1884, 33.68483), false), // 67 
                new BusCheck(new Vector3(-547.4557, -372.1765, 34.01668), false), // 68 
                new BusCheck(new Vector3(-528.7603, -347.9981, 33.96207), false), // 69 
                new BusCheck(new Vector3(-530.0774, -326.6232, 33.89759), true), // 70 
                new BusCheck(new Vector3(-546.2736, -311.8991, 33.93955), false), // 71 
                new BusCheck(new Vector3(-530.269, -291.5805, 34.12726), false), // 72 
                new BusCheck(new Vector3(-467.8711, -261.2444, 34.81455), false), // 73 
                new BusCheck(new Vector3(-384.8649, -213.7437, 35.43827), false), // 74 
                new BusCheck(new Vector3(-314.497, -188.2453, 37.89246), false), // 75 
                new BusCheck(new Vector3(-249.572, -173.7329, 39.90198), false), // 76 
                new BusCheck(new Vector3(-136.2342, -215.167, 43.56858), false), // 77 
                new BusCheck(new Vector3(-87.7426, -244.5656, 43.74351), false), // 78 
                new BusCheck(new Vector3(9.675021, -278.812, 46.40348), false), // 79 
                new BusCheck(new Vector3(157.8034, -332.6293, 43.21403), false), // 80 
                new BusCheck(new Vector3(278.173, -383.5775, 43.86136), false), // 81 
                new BusCheck(new Vector3(293.9956, -460.4826, 42.11308), false), // 82 
                new BusCheck(new Vector3(253.295, -587.212, 42.09124), false), // 83 
                new BusCheck(new Vector3(183.7153, -787.3734, 30.57219), false), // 84 
                new BusCheck(new Vector3(262.0895, -850.7108, 28.29068), false), // 85 
                new BusCheck(new Vector3(310.2831, -825.9991, 28.13679), false), // 86 
                new BusCheck(new Vector3(357.397, -690.9022, 28.12162), false), // 87 
                new BusCheck(new Vector3(438.7311, -680.3595, 27.70883), false), // 88 
                new BusCheck(new Vector3(470.6174, -602.6606, 27.37575), false), // 89 
            },
            new List<BusCheck>() // busway4
            {
                new BusCheck(new Vector3(453.8556, -585.4632, 27.37613), false), // 1 
                new BusCheck(new Vector3(421.6122, -628.5309, 27.37696), false), // 2 
                new BusCheck(new Vector3(421.6091, -664.1858, 27.7319), false), // 3 
                new BusCheck(new Vector3(386.2754, -672.6588, 28.1091), false), // 4 
                new BusCheck(new Vector3(347.6455, -685.5873, 28.21749), false), // 5 
                new BusCheck(new Vector3(294.1744, -826.3273, 28.21094), false), // 6 
                new BusCheck(new Vector3(258.0646, -924.5808, 28.04475), false), // 7 
                new BusCheck(new Vector3(225.4487, -1013.317, 28.19479), false), // 8 
                new BusCheck(new Vector3(205.5408, -1111.249, 28.2184), false), // 9 
                new BusCheck(new Vector3(209.8643, -1264.731, 28.12087), false), // 10 
                new BusCheck(new Vector3(167.1014, -1368.728, 28.22054), false), // 11 
                new BusCheck(new Vector3(117.389, -1429.787, 28.21703), false), // 12 
                new BusCheck(new Vector3(75.68818, -1487.504, 28.21913), false), // 13 
                new BusCheck(new Vector3(20.3194, -1533.67, 28.05719), true), // 14 
                new BusCheck(new Vector3(-8.654513, -1579.292, 28.21726), false), // 15 
                new BusCheck(new Vector3(-51.35416, -1622.004, 28.1502), false), // 16 
                new BusCheck(new Vector3(-124.6821, -1710.043, 28.56663), false), // 17 
                new BusCheck(new Vector3(-188.8183, -1776.236, 28.62904), false), // 18 
                new BusCheck(new Vector3(-359.8605, -1816.101, 21.61161), false), // 19 
                new BusCheck(new Vector3(-398.4728, -1782.785, 20.23422), false), // 20 
                new BusCheck(new Vector3(-429.6425, -1766.572, 19.44549), false), // 21 
                new BusCheck(new Vector3(-504.2426, -1772.536, 20.01883), false), // 22 
                new BusCheck(new Vector3(-634.8244, -1684.206, 23.67981), false), // 23 
                new BusCheck(new Vector3(-692.3253, -1611.58, 21.26535), false), // 24 
                new BusCheck(new Vector3(-672.5453, -1565.021, 15.51652), false), // 25 
                new BusCheck(new Vector3(-644.7435, -1490.931, 9.5577), false), // 26 
                new BusCheck(new Vector3(-635.9371, -1323.845, 9.532298), false), // 27 
                new BusCheck(new Vector3(-637.6625, -1281.039, 9.491567), false), // 28 
                new BusCheck(new Vector3(-665.6615, -1241.355, 9.387545), true), // 29 
                new BusCheck(new Vector3(-694.1779, -1203.753, 9.494384), false), // 30 
                new BusCheck(new Vector3(-749.1682, -1132.32, 9.453946), false), // 31 
                new BusCheck(new Vector3(-741.6571, -1108.158, 9.7827), false), // 32 
                new BusCheck(new Vector3(-682.8893, -1071.818, 14.03734), false), // 33 
                new BusCheck(new Vector3(-630.8749, -978.054, 20.19207), false), // 34 
                new BusCheck(new Vector3(-629.5759, -864.6816, 23.74202), false), // 35 
                new BusCheck(new Vector3(-629.6269, -685.7154, 29.9649), false), // 36 
                new BusCheck(new Vector3(-624.3248, -607.3519, 32.21648), false), // 37 
                new BusCheck(new Vector3(-624.3146, -574.3219, 33.72218), false), // 38 
                new BusCheck(new Vector3(-623.2767, -492.6182, 33.62238), false), // 39 
                new BusCheck(new Vector3(-620.9725, -398.1488, 33.59415), false), // 40 
                new BusCheck(new Vector3(-593.1392, -377.8099, 33.69767), false), // 41 
                new BusCheck(new Vector3(-548.579, -372.2516, 34.01007), false), // 42 
                new BusCheck(new Vector3(-529.1579, -349.2429, 33.9865), false), // 43 
                new BusCheck(new Vector3(-529.3719, -327.7787, 33.89328), true), // 44 
                new BusCheck(new Vector3(-546.7993, -312.1409, 33.96137), false), // 45 
                new BusCheck(new Vector3(-528.3561, -291.2592, 34.12596), false), // 46 
                new BusCheck(new Vector3(-464.9805, -265.4827, 34.72456), false), // 47 
                new BusCheck(new Vector3(-423.5046, -285.5312, 34.61845), false), // 48 
                new BusCheck(new Vector3(-301.887, -366.6478, 28.86615), false), // 49 
                new BusCheck(new Vector3(-236.6375, -407.36, 29.33816), false), // 50 
                new BusCheck(new Vector3(-189.6397, -398.5428, 31.38181), false), // 51 
                new BusCheck(new Vector3(-151.001, -373.3288, 32.60671), false), // 52 
                new BusCheck(new Vector3(-110.4698, -279.7641, 40.96797), false), // 53 
                new BusCheck(new Vector3(-40.77434, -264.1393, 44.93719), false), // 54 
                new BusCheck(new Vector3(8.067969, -278.4043, 46.38866), false), // 55 
                new BusCheck(new Vector3(61.09646, -302.6208, 46.00541), false), // 56 
                new BusCheck(new Vector3(155.9166, -336.3494, 43.15847), false), // 57 
                new BusCheck(new Vector3(277.7957, -383.533, 43.84637), false), // 58 
                new BusCheck(new Vector3(295.336, -439.189, 42.78767), false), // 59 
                new BusCheck(new Vector3(290.9848, -490.5533, 42.2047), false), // 60 
                new BusCheck(new Vector3(262.1586, -565.2186, 42.1925), false), // 61 
                new BusCheck(new Vector3(246.1858, -604.4952, 41.45987), false), // 62 
                new BusCheck(new Vector3(232.784, -638.3197, 38.76175), false), // 63 
                new BusCheck(new Vector3(212.0555, -694.3984, 34.94225), false), // 64 
                new BusCheck(new Vector3(199.955, -741.1058, 32.54739), false), // 65 
                new BusCheck(new Vector3(182.5596, -790.4037, 30.44635), false), // 66 
                new BusCheck(new Vector3(178.4773, -817.9296, 30.05125), false), // 67 
                new BusCheck(new Vector3(198.7472, -832.9636, 29.82287), false), // 68 
                new BusCheck(new Vector3(230.6265, -844.2143, 29.02252), false), // 69 
                new BusCheck(new Vector3(263.5145, -850.8544, 28.27174), false), // 70 
                new BusCheck(new Vector3(286.151, -853.6429, 28.08513), false), // 71 
                new BusCheck(new Vector3(304.3442, -828.9249, 28.20201), false), // 72 
                new BusCheck(new Vector3(319.3789, -800.3757, 28.13698), false), // 73 
                new BusCheck(new Vector3(336.3853, -749.7014, 28.1079), false), // 74 
                new BusCheck(new Vector3(356.8209, -692.2209, 28.12577), false), // 75 
                new BusCheck(new Vector3(378.2592, -679.9061, 28.0526), false), // 76 
                new BusCheck(new Vector3(418.054, -680.5646, 28.12943), false), // 77 
                new BusCheck(new Vector3(450.4335, -680.6489, 27.02914), false), // 78 
                new BusCheck(new Vector3(464.2, -658.661, 26.50621), false), // 79 
                new BusCheck(new Vector3(466.177, -640.4911, 27.34141), false), // 80 
                new BusCheck(new Vector3(468.84, -616.1129, 27.37415), false), // 81
            },
            new List<BusCheck>() // busway6
            {
                new BusCheck(new Vector3(452.2434, -587.5063, 27.37679), false), // 1 
                new BusCheck(new Vector3(421.4142, -639.011, 27.3762), false), // 2 
                new BusCheck(new Vector3(421.622, -663.5898, 27.70913), false), // 3 
                new BusCheck(new Vector3(386.1311, -672.2119, 28.09329), false), // 4 
                new BusCheck(new Vector3(339.8356, -689.7584, 28.22546), false), // 5 
                new BusCheck(new Vector3(293.2329, -827.4853, 28.20639), false), // 6 
                new BusCheck(new Vector3(256.9976, -926.5665, 28.07068), false), // 7 
                new BusCheck(new Vector3(221.0342, -1010.805, 28.11632), false), // 8 
                new BusCheck(new Vector3(205.3955, -1111.734, 28.21861), false), // 9 
                new BusCheck(new Vector3(209.8023, -1262.885, 28.11355), false), // 10 
                new BusCheck(new Vector3(167.6845, -1368.563, 28.22132), false), // 11 
                new BusCheck(new Vector3(124.418, -1420.834, 28.21667), false), // 12 
                new BusCheck(new Vector3(75.05229, -1488.073, 28.21492), false), // 13 
                new BusCheck(new Vector3(21.88153, -1532.233, 28.06308), true), // 14 
                new BusCheck(new Vector3(-13.4475, -1576.497, 28.12444), false), // 15 
                new BusCheck(new Vector3(-46.7314, -1573.934, 28.49072), false), // 16 
                new BusCheck(new Vector3(-94.3346, -1533.805, 32.48144), false), // 17 
                new BusCheck(new Vector3(-182.0512, -1465.002, 30.57711), false), // 18 
                new BusCheck(new Vector3(-250.7604, -1420.468, 30.13886), false), // 19 
                new BusCheck(new Vector3(-270.9648, -1193.92, 22.72335), false), // 20 
                new BusCheck(new Vector3(-275.1719, -1166.776, 21.96469), false), // 21 
                new BusCheck(new Vector3(-301.226, -1137.125, 22.2746), false), // 22 
                new BusCheck(new Vector3(-508.1477, -1075.151, 21.74836), false), // 23 
                new BusCheck(new Vector3(-528.0355, -1045.954, 21.44039), false), // 24 
                new BusCheck(new Vector3(-539.4933, -986.8386, 22.23119), false), // 25 
                new BusCheck(new Vector3(-562.0121, -956.3507, 22.30779), false), // 26 
                new BusCheck(new Vector3(-612.4644, -954.7506, 20.56333), false), // 27 
                new BusCheck(new Vector3(-630.6671, -935.9827, 21.25985), false), // 28 
                new BusCheck(new Vector3(-630.2991, -863.0883, 23.76212), false), // 29 
                new BusCheck(new Vector3(-629.4188, -684.2758, 30.03441), false), // 30 
                new BusCheck(new Vector3(-624.2628, -573.6647, 33.73887), false), // 31 
                new BusCheck(new Vector3(-623.3481, -492.4886, 33.62532), false), // 32 
                new BusCheck(new Vector3(-621.5667, -397.3529, 33.61706), false), // 33 
                new BusCheck(new Vector3(-593.8776, -377.0666, 33.70655), false), // 34 
                new BusCheck(new Vector3(-548.8806, -372.2225, 34.00932), false), // 35 
                new BusCheck(new Vector3(-529.6252, -326.6782, 33.90096), true), // 36 
                new BusCheck(new Vector3(-573.9676, -302.2077, 34.03878), false), // 37 
                new BusCheck(new Vector3(-612.9118, -324.7812, 33.72791), false), // 38 
                new BusCheck(new Vector3(-672.2838, -353.6512, 33.52838), false), // 39 
                new BusCheck(new Vector3(-773.0165, -320.6183, 35.66534), false), // 40 
                new BusCheck(new Vector3(-954.8832, -228.7624, 36.89107), false), // 41 
                new BusCheck(new Vector3(-1005.331, -203.8067, 36.71843), false), // 42 
                new BusCheck(new Vector3(-1195.436, -99.55115, 39.7301), false), // 43 
                new BusCheck(new Vector3(-1292.869, -51.96901, 45.95277), false), // 44 
                new BusCheck(new Vector3(-1354.426, -44.17735, 50.02855), true), // 45 
                new BusCheck(new Vector3(-1398.493, -43.50904, 51.48283), false), // 46 
                new BusCheck(new Vector3(-1417.898, 19.73363, 51.39941), false), // 47 
                new BusCheck(new Vector3(-1403.442, 170.0919, 56.16145), false), // 48 
                new BusCheck(new Vector3(-1278.684, 210.5041, 58.91022), false), // 49 
                new BusCheck(new Vector3(-1109.947, 256.7668, 63.53265), false), // 50 
                new BusCheck(new Vector3(-1058.386, 254.2274, 62.98872), false), // 51 
                new BusCheck(new Vector3(-875.33, 219.8016, 72.23686), false), // 52 
                new BusCheck(new Vector3(-777.5953, 211.3047, 74.71775), false), // 53 
                new BusCheck(new Vector3(-675.9929, 248.9641, 80.24601), false), // 54 
                new BusCheck(new Vector3(-564.4936, 255.0852, 81.94671), false), // 55 
                new BusCheck(new Vector3(-245.4244, 258.0541, 90.93124), false), // 56 
                new BusCheck(new Vector3(-128.1201, 249.4092, 95.06844), false), // 57 
                new BusCheck(new Vector3(9.522894, 260.4031, 108.3304), false), // 58 
                new BusCheck(new Vector3(48.06821, 281.604, 108.6653), false), // 59 
                new BusCheck(new Vector3(229.6882, 348.335, 104.4106), false), // 60 
                new BusCheck(new Vector3(398.7303, 300.6967, 101.8343), false), // 61 
                new BusCheck(new Vector3(543.4553, 249.2258, 101.9547), false), // 62 
                new BusCheck(new Vector3(739.5217, 180.306, 83.11983), false), // 63 
                new BusCheck(new Vector3(743.294, 101.0865, 78.70757), true), // 64 
                new BusCheck(new Vector3(708.2929, 39.3074, 83.12298), false), // 65 
                new BusCheck(new Vector3(716.9739, -11.14128, 82.48751), false), // 66 
                new BusCheck(new Vector3(849.0188, -94.86089, 79.06285), false), // 67 
                new BusCheck(new Vector3(921.6854, -146.491, 74.49681), false), // 68 
                new BusCheck(new Vector3(916.2418, -192.2234, 71.97924), true), // 69 
                new BusCheck(new Vector3(917.4128, -258.0396, 67.56607), false), // 70 
                new BusCheck(new Vector3(943.6136, -308.0845, 65.74582), false), // 71 
                new BusCheck(new Vector3(782.7349, -341.8244, 48.74839), false), // 72 
                new BusCheck(new Vector3(676.6306, -382.3899, 40.20858), false), // 73 
                new BusCheck(new Vector3(647.972, -382.5377, 41.45459), false), // 74 
                new BusCheck(new Vector3(495.7778, -322.7706, 44.19924), false), // 75 
                new BusCheck(new Vector3(452.1121, -337.3417, 46.41497), false), // 76 
                new BusCheck(new Vector3(340.6479, -391.3669, 44.09866), false), // 77 
                new BusCheck(new Vector3(305.6043, -412.3622, 43.92403), false), // 78 
                new BusCheck(new Vector3(288.7418, -497.2232, 42.19519), false), // 79 
                new BusCheck(new Vector3(278.995, -524.0405, 42.161), false), // 80 
                new BusCheck(new Vector3(247.0715, -602.6978, 41.56906), false), // 81 
                new BusCheck(new Vector3(182.9187, -790.1367, 30.46082), false), // 82 
                new BusCheck(new Vector3(200.5206, -833.4717, 29.77912), false), // 83 
                new BusCheck(new Vector3(262.3922, -850.6533, 28.2874), false), // 84 
                new BusCheck(new Vector3(304.0861, -828.0737, 28.20625), false), // 85 
                new BusCheck(new Vector3(356.7018, -692.379, 28.12796), false), // 86 
                new BusCheck(new Vector3(441.0269, -679.8569, 27.62088), false), // 87 
                new BusCheck(new Vector3(468.3851, -626.2559, 27.36605), false), // 88 
                new BusCheck(new Vector3(421.9406, -618.7638, 27.37685), false), // 1 
                new BusCheck(new Vector3(421.8709, -657.6866, 27.53283), false), // 2 
                new BusCheck(new Vector3(393.6165, -673.4533, 28.07841), false), // 3 
                new BusCheck(new Vector3(346.8577, -690.0728, 28.22062), false), // 5 
                new BusCheck(new Vector3(298.8752, -809.5145, 28.21675), false), // 6 
                new BusCheck(new Vector3(258.0472, -924.6455, 28.04573), false), // 7 
                new BusCheck(new Vector3(225.6352, -1013.022, 28.19472), false), // 8 
                new BusCheck(new Vector3(208.7699, -1064.417, 28.13343), false), // 9 
                new BusCheck(new Vector3(205.5882, -1111.78, 28.21763), false), // 10 
                new BusCheck(new Vector3(202.4902, -1154.195, 28.1399), false), // 11 
                new BusCheck(new Vector3(209.3809, -1266.229, 28.10783), false), // 12 
                new BusCheck(new Vector3(168.1012, -1368.395, 28.22142), false), // 13 
                new BusCheck(new Vector3(100.2492, -1457.411, 28.21803), false), // 14 
                new BusCheck(new Vector3(75.45205, -1487.638, 28.21903), false), // 15 
                new BusCheck(new Vector3(20.91828, -1533.513, 28.06536), true), // 16 
                new BusCheck(new Vector3(-10.86836, -1580.971, 28.21745), false), // 17 
                new BusCheck(new Vector3(-50.68108, -1621.147, 28.14725), false), // 18 
                new BusCheck(new Vector3(-120.4083, -1712.595, 28.59435), false), // 19 
                new BusCheck(new Vector3(-109.4427, -1765.048, 28.65141), false), // 20 
                new BusCheck(new Vector3(-38.42766, -1824.238, 25.06485), false), // 21 
                new BusCheck(new Vector3(42.46912, -1891.335, 20.86017), false), // 22 
                new BusCheck(new Vector3(75.74171, -1889.607, 21.11408), false), // 23 
                new BusCheck(new Vector3(154.3352, -1796.16, 27.77178), false), // 24 
                new BusCheck(new Vector3(154.3352, -1796.16, 27.77178), false), // 24 
                new BusCheck(new Vector3(193.6535, -1753.086, 27.6894), false), // 25 
                new BusCheck(new Vector3(249.6708, -1708.615, 27.93861), false), // 26 
                new BusCheck(new Vector3(266.8677, -1687.659, 28.10419), false), // 27 
                new BusCheck(new Vector3(259.3495, -1655.42, 28.13954), false), // 28 
                new BusCheck(new Vector3(218.8917, -1618.119, 28.133), false), // 29 
                new BusCheck(new Vector3(220.5459, -1581.312, 28.1322), false), // 30 
                new BusCheck(new Vector3(295.3427, -1527.888, 28.2114), false), // 31 
                new BusCheck(new Vector3(306.0768, -1495.258, 28.21554), false), // 32 
                new BusCheck(new Vector3(261.2666, -1450.514, 28.14244), false), // 33 
                new BusCheck(new Vector3(261.593, -1425.938, 28.17628), false), // 34 
                new BusCheck(new Vector3(325.1118, -1348.65, 31.24809), false), // 35 
                new BusCheck(new Vector3(412.9283, -1262.451, 31.1344), false), // 36 
                new BusCheck(new Vector3(469.9149, -1264.976, 28.43671), true), // 37 
                new BusCheck(new Vector3(500.1995, -1286.848, 28.15525), false), // 38 
                new BusCheck(new Vector3(524.0394, -1337.825, 28.09183), false), // 39 
                new BusCheck(new Vector3(532.9698, -1407.265, 28.13147), false), // 40 
                new BusCheck(new Vector3(574.0267, -1440.002, 28.55653), false), // 41 
                new BusCheck(new Vector3(677.1075, -1445.318, 29.79763), false), // 42 
                new BusCheck(new Vector3(765.7747, -1440.348, 26.55122), false), // 43 
                new BusCheck(new Vector3(800.4399, -1410.418, 26.14221), false), // 44 
                new BusCheck(new Vector3(806.0775, -1332.82, 25.07283), false), // 45 
                new BusCheck(new Vector3(806.1255, -1330.809, 25.07724), false), // 45 
                new BusCheck(new Vector3(807.5174, -1262.249, 25.22744), false), // 46 
                new BusCheck(new Vector3(807.001, -1196.299, 26.16691), true), // 47 
                new BusCheck(new Vector3(804.5488, -1167.321, 27.55787), false), // 48 
                new BusCheck(new Vector3(801.2711, -1119.821, 28.02073), false), // 49 
                new BusCheck(new Vector3(816.2949, -1088.402, 27.44702), false), // 50 
                new BusCheck(new Vector3(939.2841, -1088.803, 34.32776), false), // 51 
                new BusCheck(new Vector3(997.1431, -1012.801, 40.98442), false), // 52 
                new BusCheck(new Vector3(1013.027, -990.4582, 41.29606), false), // 53 
                new BusCheck(new Vector3(1132.458, -955.3203, 46.6568), false), // 54 
                new BusCheck(new Vector3(1151.959, -997.3826, 44.16554), false), // 55 
                new BusCheck(new Vector3(1187.216, -1100.158, 38.48834), false), // 56 
                new BusCheck(new Vector3(1237.02, -1224.887, 34.41777), false), // 57 
                new BusCheck(new Vector3(1224.449, -1377.012, 34.01226), false), // 58 
                new BusCheck(new Vector3(1203.998, -1421.756, 33.91041), false), // 59 
                new BusCheck(new Vector3(1097.563, -1428.605, 35.23575), false), // 60 
                new BusCheck(new Vector3(824.2581, -1438.221, 26.24022), false), // 61 
                new BusCheck(new Vector3(804.0295, -1484.91, 26.7456), false), // 62 
                new BusCheck(new Vector3(816.4278, -1721.253, 28.14004), false), // 63 
                new BusCheck(new Vector3(763.9579, -2035.383, 28.1831), false), // 64 
                new BusCheck(new Vector3(742.4531, -2162.879, 28.06165), true), // 65 
                new BusCheck(new Vector3(746.2746, -2225.885, 28.21369), false), // 66 
                new BusCheck(new Vector3(725.7681, -2395.195, 19.78458), false), // 67 
                new BusCheck(new Vector3(624.7829, -2492.191, 15.96053), false), // 68 
                new BusCheck(new Vector3(526.284, -2435.844, 13.34998), false), // 69 
                new BusCheck(new Vector3(529.1344, -2542.544, 4.877124), false), // 70 
                new BusCheck(new Vector3(668.3756, -2826.031, 5.04612), false), // 71 
                new BusCheck(new Vector3(630.8576, -3003.073, 4.919397), false), // 72 
                new BusCheck(new Vector3(582.1158, -3010.819, 4.921851), true), // 73 
                new BusCheck(new Vector3(485.5909, -3012.602, 4.917102), false), // 74 
                new BusCheck(new Vector3(478.0564, -2983.276, 4.925123), false), // 75 
                new BusCheck(new Vector3(639.1627, -2983.211, 4.921716), false), // 76 
                new BusCheck(new Vector3(672.9695, -2871.464, 5.066907), false), // 77 
                new BusCheck(new Vector3(570.3653, -2560.852, 5.390557), false), // 78 
                new BusCheck(new Vector3(612.9646, -2510.968, 15.77748), false), // 79 
                new BusCheck(new Vector3(706.2491, -2485.117, 19.0746), false), // 80 
                new BusCheck(new Vector3(759.6096, -2356.076, 22.56039), false), // 81 
                new BusCheck(new Vector3(782.4882, -2094.873, 28.1325), false), // 82 
                new BusCheck(new Vector3(815.0789, -1888.762, 28.10884), false), // 83 
                new BusCheck(new Vector3(839.2156, -1701.102, 28.21691), false), // 84 
                new BusCheck(new Vector3(817.5537, -1503.17, 27.2326), false), // 85 
                new BusCheck(new Vector3(765.5114, -1428.388, 26.49767), false), // 86 
                new BusCheck(new Vector3(566.6123, -1428.193, 28.39441), false), // 87 
                new BusCheck(new Vector3(534.1582, -1344.483, 28.14856), false), // 88 
                new BusCheck(new Vector3(510.3243, -708.2892, 23.78441), false), // 89 
                new BusCheck(new Vector3(479.5815, -674.241, 25.14934), false), // 90 
                new BusCheck(new Vector3(467.0784, -634.6727, 27.36381), false), // 91 
            },
            new List<BusCheck>()
            {
                new BusCheck(new Vector3(443.6877, -585.6654, 26.96806), false), // 1 
                new BusCheck(new Vector3(421.5607, -646.3228, 26.96887), false), // 2 
                new BusCheck(new Vector3(383.5445, -670.6879, 27.66392), false), // 3 
                new BusCheck(new Vector3(347.9878, -687.4364, 27.812), false), // 4 
                new BusCheck(new Vector3(293.0769, -829.8888, 27.79329), false), // 5 
                new BusCheck(new Vector3(256.3426, -929.2927, 27.70035), false), // 6 
                new BusCheck(new Vector3(224.5502, -1015.327, 27.79179), false), // 7 
                new BusCheck(new Vector3(205.1135, -1114.731, 27.80716), false), // 8 
                new BusCheck(new Vector3(209.7603, -1268.978, 27.72885), false), // 9 
                new BusCheck(new Vector3(165.0832, -1372.029, 27.81084), false), // 10 
                new BusCheck(new Vector3(131.8338, -1412.747, 27.80566), false), // 11 
                new BusCheck(new Vector3(72.20358, -1492.503, 27.79235), false), // 12 
                new BusCheck(new Vector3(20.02134, -1534.584, 27.66285), true), // 13 
                new BusCheck(new Vector3(-8.335826, -1585.989, 27.80775), false), // 14 
                new BusCheck(new Vector3(-13.36246, -1633.336, 27.75887), false), // 15 
                new BusCheck(new Vector3(24.70691, -1665.503, 27.75342), false), // 16 
                new BusCheck(new Vector3(94.80923, -1724.198, 27.39403), false), // 17 
                new BusCheck(new Vector3(150.4926, -1771.009, 27.41319), false), // 18 
                new BusCheck(new Vector3(247.373, -1852.126, 25.20545), false), // 19 
                new BusCheck(new Vector3(294.5547, -1891.913, 25.38673), false), // 20 
                new BusCheck(new Vector3(337.9873, -1928.695, 23.1462), false), // 21 
                new BusCheck(new Vector3(394.4581, -1978.488, 22.099), false), // 22 
                new BusCheck(new Vector3(456.8414, -2044.886, 22.73396), false), // 23 
                new BusCheck(new Vector3(456.2177, -2077.327, 21.36689), false), // 24 
                new BusCheck(new Vector3(433.944, -2113.04, 18.84294), false), // 25 
                new BusCheck(new Vector3(394.7001, -2157.282, 14.93311), false), // 26 
                new BusCheck(new Vector3(353.9064, -2214.09, 10.36401), false), // 27 
                new BusCheck(new Vector3(346.6998, -2345.49, 8.602489), false), // 28 
                new BusCheck(new Vector3(333.6717, -2447.544, 5.654027), false), // 29 
                new BusCheck(new Vector3(329.6519, -2494.301, 3.927794), false), // 30 
                new BusCheck(new Vector3(347.2268, -2508.839, 4.478691), false), // 31 
                new BusCheck(new Vector3(412.9386, -2510.853, 11.871), false), // 32 
                new BusCheck(new Vector3(498.1788, -2536.203, 4.948865), false), // 33 
                new BusCheck(new Vector3(564.9095, -2569.475, 5.0106), false), // 34 
                new BusCheck(new Vector3(664.5588, -2765.938, 4.646408), false), // 35 
                new BusCheck(new Vector3(665.5758, -2939.92, 4.517673), false), // 36 
                new BusCheck(new Vector3(648.3298, -2946.5, 4.510792), false), // 37 
                new BusCheck(new Vector3(592.355, -2958.057, 4.512614), false), // 38 
                new BusCheck(new Vector3(602.8403, -3018.18, 4.511153), true), // 39 
                new BusCheck(new Vector3(637.4865, -3004.125, 4.513565), false), // 40 
                new BusCheck(new Vector3(673.1204, -2898.62, 4.676573), false), // 41 
                new BusCheck(new Vector3(640.3093, -2680.34, 4.542529), false), // 42 
                new BusCheck(new Vector3(585.0902, -2576.085, 4.593889), false), // 43 
                new BusCheck(new Vector3(603.8317, -2513.08, 15.27206), false), // 44 
                new BusCheck(new Vector3(709.5429, -2484.908, 18.66641), false), // 45 
                new BusCheck(new Vector3(753.3414, -2433.371, 18.45189), false), // 46 
                new BusCheck(new Vector3(776.8031, -2215.839, 27.72613), false), // 47 
                new BusCheck(new Vector3(795.0637, -2020.518, 27.74448), false), // 48 
                new BusCheck(new Vector3(837.6479, -1807.675, 27.48327), true), // 49 
                new BusCheck(new Vector3(843.0119, -1716.738, 27.75781), false), // 50 
                new BusCheck(new Vector3(853.5858, -1603.594, 30.37821), false), // 51 
                new BusCheck(new Vector3(830.1688, -1522.482, 27.31706), false), // 52 
                new BusCheck(new Vector3(806.5134, -1455.481, 25.64765), false), // 53 
                new BusCheck(new Vector3(805.7725, -1398.135, 25.62822), false), // 55 
                new BusCheck(new Vector3(800.9803, -1255.748, 24.88928), false), // 56 
                new BusCheck(new Vector3(807.4732, -1195.799, 25.7757), true), // 57 
                new BusCheck(new Vector3(799.62, -1165.082, 27.27798), false), // 58 
                new BusCheck(new Vector3(780.0538, -1025.732, 24.65733), false), // 59 
                new BusCheck(new Vector3(761.7024, -1003.466, 24.61902), false), // 60 
                new BusCheck(new Vector3(568.8751, -1025.474, 35.50324), false), // 61 
                new BusCheck(new Vector3(416.2618, -1045.127, 28.02459), false), // 62 
                new BusCheck(new Vector3(394.9145, -1084.221, 27.82005), false), // 63 
                new BusCheck(new Vector3(395.4652, -1118.03, 27.88565), false), // 64 
                new BusCheck(new Vector3(430.8953, -1134.403, 27.86587), false), // 65 
                new BusCheck(new Vector3(485.5118, -1134.136, 27.87874), false), // 66 
                new BusCheck(new Vector3(498.7965, -1151.116, 27.74828), false), // 67 
                new BusCheck(new Vector3(498.8574, -1245.841, 27.70419), false), // 68 
                new BusCheck(new Vector3(449.797, -1249.931, 28.60692), true), // 69 
                new BusCheck(new Vector3(415.6601, -1257.142, 30.52675), false), // 70 
                new BusCheck(new Vector3(353.4924, -1306.899, 30.78134), false), // 71 
                new BusCheck(new Vector3(332.9001, -1306.49, 30.30584), false), // 72 
                new BusCheck(new Vector3(244.375, -1294.459, 27.67779), false), // 73 
                new BusCheck(new Vector3(233.0126, -1269.025, 27.66193), false), // 74 
                new BusCheck(new Vector3(218.3602, -1148.535, 27.80059), false), // 75 
                new BusCheck(new Vector3(218.8137, -1066.479, 27.68471), false), // 76 
                new BusCheck(new Vector3(237.1028, -1016.565, 27.77794), false), // 77 
                new BusCheck(new Vector3(253.5884, -966.584, 27.79571), false), // 78 
                new BusCheck(new Vector3(286.5269, -878.4503, 27.74542), false), // 79 
                new BusCheck(new Vector3(303.7062, -831.5664, 27.7911), false), // 80 
                new BusCheck(new Vector3(325.1037, -786.3784, 27.6881), false), // 81 
                new BusCheck(new Vector3(345.5882, -723.6211, 27.70319), false), // 82 
                new BusCheck(new Vector3(358.49, -687.8741, 27.71537), false), // 83 
                new BusCheck(new Vector3(378.0378, -679.6871, 27.64757), false), // 84 
                new BusCheck(new Vector3(434.3148, -679.9254, 27.51877), false), // 85 
                new BusCheck(new Vector3(462.9022, -661.4355, 25.96049), false), // 86 
                new BusCheck(new Vector3(469.7245, -615.9445, 26.9684), false), // 87 
            },
            new List<BusCheck>()
            {
                new BusCheck(new Vector3(446.6861, -585.6976, 27.00236), false), // 1 
                new BusCheck(new Vector3(421.7016, -644.7448, 27.00387), false), // 2 
                new BusCheck(new Vector3(421.5157, -662.2115, 27.32454), false), // 3 
                new BusCheck(new Vector3(382.6215, -670.8714, 27.72272), false), // 4 
                new BusCheck(new Vector3(347.8421, -684.2863, 27.84402), false), // 5 
                new BusCheck(new Vector3(286.7675, -830.9046, 27.73273), false), // 6 
                new BusCheck(new Vector3(261.4495, -838.7652, 27.92509), false), // 7 
                new BusCheck(new Vector3(197.8796, -822.0228, 29.5722), false), // 8 
                new BusCheck(new Vector3(59.43143, -771.5594, 30.21667), false), // 9 
                new BusCheck(new Vector3(9.114623, -753.1033, 30.33282), false), // 10 
                new BusCheck(new Vector3(-94.35464, -711.0247, 33.25034), false), // 11 
                new BusCheck(new Vector3(-230.318, -660.0676, 31.82929), false), // 12 
                new BusCheck(new Vector3(-238.2605, -632.8104, 32.12958), false), // 13 
                new BusCheck(new Vector3(-202.9594, -508.6712, 33.27271), false), // 14 
                new BusCheck(new Vector3(-205.3311, -444.3093, 31.53565), false), // 15 
                new BusCheck(new Vector3(-259.655, -355.3008, 28.56204), false), // 16 
                new BusCheck(new Vector3(-272.332, -330.6287, 28.57689), false), // 17 
                new BusCheck(new Vector3(-402.2968, -246.765, 34.7491), false), // 18 
                new BusCheck(new Vector3(-437.8106, -239.0391, 34.70686), false), // 19 
                new BusCheck(new Vector3(-524.1899, -267.2105, 33.79911), true), // 20 
                new BusCheck(new Vector3(-549.8785, -283.0546, 33.66624), false), // 21 
                new BusCheck(new Vector3(-612.0635, -330.7424, 33.34597), false), // 22 
                new BusCheck(new Vector3(-636.6931, -394.1144, 33.32231), false), // 23 
                new BusCheck(new Vector3(-639.257, -458.1576, 33.32069), false), // 24 
                new BusCheck(new Vector3(-659.2593, -470.9107, 33.16987), false), // 25 
                new BusCheck(new Vector3(-886.3428, -504.7376, 20.08286), false), // 26 
                new BusCheck(new Vector3(-1098.359, -609.2435, 14.11171), false), // 27 
                new BusCheck(new Vector3(-1612.887, -718.8878, 9.712423), false), // 28 
                new BusCheck(new Vector3(-1853.955, -550.7711, 10.09708), false), // 29 
                new BusCheck(new Vector3(-2146.804, -349.8192, 11.70847), false), // 30 
                new BusCheck(new Vector3(-2304.239, -314.5258, 12.22392), false), // 31 
                new BusCheck(new Vector3(-2646.875, -82.55535, 16.42738), false), // 32 
                new BusCheck(new Vector3(-3014.273, 166.0826, 14.14081), false), // 33 
                new BusCheck(new Vector3(-2984.149, 417.7342, 13.45377), false), // 34 
                new BusCheck(new Vector3(-3093.051, 793.4437, 17.45447), false), // 35 
                new BusCheck(new Vector3(-3104.435, 1097.097, 18.97407), true), // 36 
                new BusCheck(new Vector3(-3100.077, 1180.312, 18.8608), false), // 37 
                new BusCheck(new Vector3(-3013.796, 1439.467, 24.82408), false), // 38 
                new BusCheck(new Vector3(-3032.274, 1731.693, 35.25812), false), // 39 
                new BusCheck(new Vector3(-2980.151, 2022.194, 33.36716), false), // 40 
                new BusCheck(new Vector3(-2717.431, 2277.759, 17.9179), false), // 41 
                new BusCheck(new Vector3(-2675.636, 2476.249, 15.16318), false), // 42 
                new BusCheck(new Vector3(-2626.239, 2814.853, 15.16363), false), // 43 
                new BusCheck(new Vector3(-2554.164, 3398.625, 11.73912), false), // 44 
                new BusCheck(new Vector3(-2461.012, 3709.885, 13.72708), true), // 45 
                new BusCheck(new Vector3(-2408.56, 3864.183, 22.7857), false), // 46 
                new BusCheck(new Vector3(-2283.584, 4198.521, 39.39719), false), // 47 
                new BusCheck(new Vector3(-2169.977, 4419.877, 58.82072), false), // 48 
                new BusCheck(new Vector3(-2011.749, 4501.045, 55.55472), false), // 49 
                new BusCheck(new Vector3(-1763.015, 4748.355, 55.62331), false), // 50 
                new BusCheck(new Vector3(-1417.936, 5065.487, 59.68685), false), // 51 
                new BusCheck(new Vector3(-1295.887, 5230.222, 52.87307), false), // 52 
                new BusCheck(new Vector3(-1118.212, 5300.123, 49.21911), false), // 53 
                new BusCheck(new Vector3(-915.248, 5414.611, 35.58352), false), // 54 
                new BusCheck(new Vector3(-753.3878, 5495.157, 33.70341), false), // 55 
                new BusCheck(new Vector3(-448.4627, 5901.2, 31.3485), false), // 56 
                new BusCheck(new Vector3(-232.4834, 6136.175, 29.69926), false), // 57 
                new BusCheck(new Vector3(-151.3444, 6211.825, 29.69864), true), // 58 
                new BusCheck(new Vector3(-107.5252, 6257.996, 29.68674), false), // 59 
                new BusCheck(new Vector3(260.1705, 6560.667, 29.34554), false), // 60 
                new BusCheck(new Vector3(573.419, 6536.126, 26.44849), false), // 61 
                new BusCheck(new Vector3(926.0767, 6483.094, 19.65213), false), // 62 
                new BusCheck(new Vector3(1270.545, 6484.433, 18.93892), false), // 64 
                new BusCheck(new Vector3(1571.767, 6402.569, 23.46802), false), // 65 
                new BusCheck(new Vector3(1936.01, 6250.097, 42.06968), false), // 66 
                new BusCheck(new Vector3(2126.284, 6017.464, 49.66409), false), // 67 
                new BusCheck(new Vector3(2402.757, 5710.199, 43.88626), false), // 68 
                new BusCheck(new Vector3(2546.62, 5334.474, 43.07608), false), // 69 
                new BusCheck(new Vector3(2643.089, 4966.605, 43.23901), false), // 70 
                new BusCheck(new Vector3(2760.365, 4427.607, 46.93263), false), // 71 
                new BusCheck(new Vector3(2710.661, 4383.659, 46.01754), false), // 72 
                new BusCheck(new Vector3(2502.408, 4118.359, 36.95085), false), // 73 
                new BusCheck(new Vector3(2284.848, 3852.638, 33.02863), false), // 74 
                new BusCheck(new Vector3(2074.018, 3730.402, 31.54861), false), // 75 
                new BusCheck(new Vector3(2047.203, 3756.253, 30.8865), false), // 76 
                new BusCheck(new Vector3(1984.971, 3739.775, 30.95483), false), // 77 
                new BusCheck(new Vector3(1856.94, 3669.111, 32.49074), true), // 78 
                new BusCheck(new Vector3(1750.066, 3599.518, 33.45465), false), // 79 
                new BusCheck(new Vector3(1688.499, 3518.189, 34.85237), false), // 80 
                new BusCheck(new Vector3(1781.543, 3354.392, 38.96047), false), // 81 
                new BusCheck(new Vector3(2071.827, 3062.378, 44.71134), false), // 82 
                new BusCheck(new Vector3(2264.338, 3002.298, 44.05603), false), // 83 
                new BusCheck(new Vector3(2359.519, 2951.845, 47.47613), false), // 84 
                new BusCheck(new Vector3(2248.539, 2812.104, 41.71938), false), // 85 
                new BusCheck(new Vector3(1918.114, 2502.823, 53.05881), false), // 86 
                new BusCheck(new Vector3(1722.671, 1638.278, 81.25053), false), // 87 
                new BusCheck(new Vector3(1451.71, 769.3271, 75.82732), false), // 88 
                new BusCheck(new Vector3(1188.885, 490.0342, 80.19238), false), // 89 
                new BusCheck(new Vector3(707.7386, -114.3767, 51.56673), false), // 90 
                new BusCheck(new Vector3(522.7889, -396.2439, 30.5106), false), // 91 
                new BusCheck(new Vector3(363.8746, -645.1902, 27.79677), false), // 92 
                new BusCheck(new Vector3(381.0358, -678.856, 27.75814), false), // 93 
                new BusCheck(new Vector3(440.7911, -680.0443, 27.26101), false), // 94 
                new BusCheck(new Vector3(465.142, -655.6293, 26.28402), false), // 95 
                new BusCheck(new Vector3(470.0535, -604.1998, 27.00337), false), // 96 
            },
        };

        #region BusStations
        private static Dictionary<string, Vector3> BusStations = new Dictionary<string, Vector3>()
        {
            { "LSPD", new Vector3(394.8946, -990.8792, 30.60689) },
            { "Main Square", new Vector3(-528.8386, -328.6082, 36.34783) },
            { "FIB", new Vector3(-1621.519, -532.9644, 35.70459) },
            { "West Side", new Vector3(19.75618, -1533.853, 30.54906) },
            { "Airport", new Vector3(-1032.82, -2723.92, 14.99705) },
            { "Airport Hotel", new Vector3(-888.1733, -2186.11, 9.900888) },
            { "Driving School", new Vector3(-663.498, -1244.046, 11.90458) },
            { "Lawn Mower", new Vector3(-1354.111, -43.5153, 52.53339) },
            { "Power Station", new Vector3(740.4898, 100.5469, 81.29053) },
            { "Taxi", new Vector3(918.1192, -188.8451, 74.84467) },
            { "Truck Station", new Vector3(602.8403, -3018.18, 6.131153) },
            { "East Side", new Vector3(837.6479, -1807.675, 29.10327) },
            { "Collector", new Vector3(807.4769, -1195.802, 27.39124) },
            { "Mechanic", new Vector3(449.7962, -1249.931, 30.22602) },
            { "Chumash", new Vector3(-3104.435, 1097.097, 20.59407) },
            { "Paleto Bay", new Vector3(-151.3444, 6211.825, 31.31864) },
            { "Sandy Shores", new Vector3(1856.94, 3669.111, 34.11074) },
        };
        #endregion

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStartHandler()
        {
            try
            {
                for (int a = 0; a < BusWays.Count; a++)
                {
                    for (int x = 0; x < BusWays[a].Count; x++)
                    {
                        var col = NAPI.ColShape.CreateCylinderColShape(BusWays[a][x].Pos, 4, 3, 0);
                        col.OnEntityEnterColShape += busCheckpointEnterWay;
                        col.SetData("WORKWAY", a);
                        col.SetData("NUMBER", x);
                    }
                }

                foreach (var station in BusStations)
                    NAPI.TextLabel.CreateTextLabel($"~y~Bus Station\n~g~{station.Key}", station.Value, 30f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static List<CarInfo> CarInfos = new List<CarInfo>();
        public static void busCarsSpawner()
        {
            // создаём автобусы
            for (int a = 0; a < CarInfos.Count; a++)
            {
                var veh = NAPI.Vehicle.CreateVehicle(CarInfos[a].Model, CarInfos[a].Position, CarInfos[a].Rotation.Z, CarInfos[a].Color1, CarInfos[a].Color2, CarInfos[a].Number);
                Core.VehicleStreaming.SetEngineState(veh, false);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "WORK", 4);
                NAPI.Data.SetEntityData(veh, "TYPE", "BUS");
                NAPI.Data.SetEntityData(veh, "NUMBER", a);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
        }

        public static void onPlayerDissconnectedHandler(Client player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 4 &&
                    NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    respawnBusCar(vehicle);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void respawnBusCar(Vehicle veh)
        {
            try
            {
                int i = NAPI.Data.GetEntityData(veh, "NUMBER");

                NAPI.Entity.SetEntityPosition(veh, CarInfos[i].Position);
                NAPI.Entity.SetEntityRotation(veh, CarInfos[i].Rotation);
                VehicleManager.RepairCar(veh);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
                NAPI.Data.SetEntityData(veh, "WORK", 4);
                NAPI.Data.SetEntityData(veh, "TYPE", "BUS");
                NAPI.Data.SetEntityData(veh, "NUMBER", i);
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            }
            catch (Exception e) { Log.Write("respawnBusCar: " + e.Message, nLog.Type.Error); }
        }

        public static Vector3 GetNearestStation(Vector3 position)
        {
            Vector3 station = BusStations["LSPD"];
            foreach (var pos in BusStations.Values)
            {
                if (position.DistanceTo(pos) < position.DistanceTo(station))
                    station = pos;
            }
            return station;
        }

        #region BusWays

        private static void busCheckpointEnterWay(ColShape shape, Client player)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(player)) return;
                var vehicle = player.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "BUS") return;
                if (Main.Players[player].WorkID != 4 || !player.GetData("ON_WORK") || player.GetData("WORKWAY") != shape.GetData("WORKWAY")) return;
                var way = player.GetData("WORKWAY");

                if (shape.GetData("NUMBER") != player.GetData("WORKCHECK")) return;
                var check = NAPI.Data.GetEntityData(player, "WORKCHECK");

                if (player.GetData("BUS_ONSTOP") == true) return;
                if (!BusWays[way][check].IsStop)
                {
                    if (NAPI.Data.GetEntityData(player, "WORKCHECK") != check) return;
                    if (check + 1 != BusWays[way].Count) check++;
                    else check = 0;

                    var direction = (check + 1 != BusWays[way].Count) ? BusWays[way][check + 1].Pos - new Vector3(0, 0, 0.12) : BusWays[way][0].Pos - new Vector3(0, 0, 1.12);
                    var color = (BusWays[way][check].IsStop) ? new Color(255, 255, 255) : new Color(255, 0, 0);
                    Trigger.ClientEvent(player, "createCheckpoint", 3, 1, BusWays[way][check].Pos - new Vector3(0, 0, 1.12), 4, 0, color.Red, color.Green, color.Blue, direction);
                    Trigger.ClientEvent(player, "createWaypoint", BusWays[way][check].Pos.X, BusWays[way][check].Pos.Y);
                    Trigger.ClientEvent(player, "createWorkBlip", BusWays[way][check].Pos);

                    NAPI.Data.SetEntityData(player, "WORKCHECK", check);
                    var payment = Convert.ToInt32(BuswaysPayments[way] * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                    //NAPI.Data.SetEntityData(player, "PAYMENT", NAPI.Data.GetEntityData(player, "PAYMENT") + payment);
                    MoneySystem.Wallet.Change(player, payment);
                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"busCheck");
                }
                else
                {
                    if (NAPI.Data.GetEntityData(player, "WORKCHECK") != check) return;
                    Trigger.ClientEvent(player, "deleteCheckpoint", 3, 0);
                    Trigger.ClientEvent(player, "deleteWorkBlip");

                    NAPI.Data.SetEntityData(player, "BUS_ONSTOP", true);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Остановка. Через 10 секунд Вы сможете продолжить маршрут", 3000);
                    player.SetData("BUS_TIMER", Timers.StartOnce(10000, () => timer_busStop(player, way, check)));

                    foreach (var p in Main.GetPlayersInRadiusOfPosition(player.Position, 30))
                        p.SendChatMessage("!{#3ADF00}Через 10 секунд отходит автобус, следующий по маршруту " + BusWaysNames[way]);
                }
            }
            catch (Exception ex) { Log.Write("busCheckpointEnterWay: " + ex.Message, nLog.Type.Error); }
        }

        private static void timer_busStop(Client player, int way, int check)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    NAPI.Data.SetEntityData(player, "BUS_ONSTOP", false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Можете ехать дальше", 3000);
                    var payment = Convert.ToInt32(BuswaysPayments[way] * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                    //NAPI.Data.SetEntityData(player, "PAYMENT", NAPI.Data.GetEntityData(player, "PAYMENT") + payment);
                    MoneySystem.Wallet.Change(player, payment);
                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"busCheck");
                    if (check + 1 != BusWays[way].Count) check++;
                    else check = 0;

                    var direction = (check + 1 < BusWays[way].Count) ? BusWays[way][check + 1].Pos - new Vector3(0, 0, 0.12) : BusWays[way][0].Pos - new Vector3(0, 0, 1.12);
                    var color = (BusWays[way][check].IsStop) ? new Color(255, 255, 255) : new Color(255, 0, 0);
                    NAPI.ClientEvent.TriggerClientEvent(player, "createCheckpoint", 3, 1, BusWays[way][check].Pos - new Vector3(0, 0, 1.12), 4, 0, color.Red, color.Green, color.Blue, direction);
                    NAPI.ClientEvent.TriggerClientEvent(player, "createWaypoint", BusWays[way][check].Pos.X, BusWays[way][check].Pos.Y);
                    NAPI.ClientEvent.TriggerClientEvent(player, "createWorkBlip", BusWays[way][check].Pos);

                    player.SetData("WORKCHECK", check);
                    //Main.StopT(player.GetData("BUS_TIMER"), "timer_23");
                    Timers.Stop(player.GetData("BUS_TIMER"));
                    player.ResetData("BUS_TIMER");

                    foreach (var p in Main.GetPlayersInRadiusOfPosition(player.Position, 30))
                        p.SendChatMessage("!{#3ADF00}Автобус отправляется по маршруту " + BusWaysNames[way]);
                }
                catch (Exception e)
                {
                    Log.Write("EXCEPTION AT \"TIMER_BUS_STOP\":\n" + e.ToString(), nLog.Type.Error);
                }
            });
        }
        #endregion

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Client player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "BUS" &&
                    Main.Players[player].WorkID == 4 &&
                    NAPI.Data.GetEntityData(player, "ON_WORK") &&
                    NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 60 секунд, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_24");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "BUS_EXIT_CAR_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.Start(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            } catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Client player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_25");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 60)
                    {
                        respawnBusCar(vehicle);

                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        Trigger.ClientEvent(player, "deleteCheckpoint", 3, 0);
                        Trigger.ClientEvent(player, "deleteWorkBlip");

                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_26");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        player.SetData("PAYMENT", 0);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                } catch(Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle:\n" + e.ToString(), nLog.Type.Error);
                }
            });
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Client player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "BUS") return;
                if (player.VehicleSeat == -1)
                {
                    if (!Main.Players[player].Licenses[2])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (Main.Players[player].WorkID == 4)
                    {
                        if (vehicle.GetData("DRIVER") == null)
                        {
                            if (player.GetData("WORK") == null)
                            {
                                if (Main.Players[player].Money >= BusRentCost)
                                {
                                    Trigger.ClientEvent(player, "openDialog", "BUS_RENT", $"Арендовать автобус за ${BusRentCost}?");
                                }
                                else
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает " + (BusRentCost - Main.Players[player].Money) + "$ на аренду автобуса", 3000);
                                    VehicleManager.WarpPlayerOutOfVehicle(player);
                                }
                            }
                            else
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный автобус", 3000);
                        }
                        else
                        {
                            if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У автобуса уже есть водитель", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                            }
                            else
                                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                        }
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете водителем автобуса. Устроиться можно в мэрии", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
                else
                {
                    if (NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                    {
                        var price = 30;
                        if (Main.Players[player].Money >= price)
                        {
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы заплатили за проезд {price}$", 3000);
                            MoneySystem.Wallet.Change(player, -price);
                            Fractions.Stocks.fracStocks[6].Money += price;
                            GameLog.Money($"player({Main.Players[player].UUID})", $"frac(6)", price, $"busPay");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств для оплаты проезда", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В автобусе сейчас нет водителя", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
            } catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
            
        }

        public static void acceptBusRent(Client player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) && player.VehicleSeat == -1 && player.Vehicle.GetData("TYPE") == "BUS")
            {
                var ways = new Dictionary<int, int>
                        {
                            { 0, 0 },
                            { 1, 0 },
                            { 2, 0 },
                            { 3, 0 },
                            { 4, 0 },
                            { 5, 0 }
                        };
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].WorkID != 4 || !p.GetData("ON_WORK")) continue;
                    ways[p.GetData("WORKWAY")]++;
                }

                var way = -1;
                for (int i = 0; i < ways.Count; i++)
                    if (ways[i] == 0)
                    {
                        way = i;
                        break;
                    }
                if (way == -1)
                {
                    for (int i = 0; i < ways.Count; i++)
                        if (ways[i] == 1)
                        {
                            way = i;
                            break;
                        }
                }
                if (way == -1) way = 0;

                var vehicle = player.Vehicle;
                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                NAPI.Data.SetEntityData(player, "ON_WORK", true);
                MoneySystem.Wallet.Change(player, -BusRentCost);
                GameLog.Money($"player({Main.Players[player].UUID})", $"server", BusRentCost, $"busRent");

                Core.VehicleStreaming.SetEngineState(vehicle, true);
                NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
                NAPI.Data.SetEntityData(vehicle, "ON_WORK", true);
                NAPI.Data.SetEntityData(vehicle, "DRIVER", player);

                NAPI.Data.SetEntityData(player, "WORKWAY", way);
                NAPI.Data.SetEntityData(player, "WORKCHECK", 0);
                Trigger.ClientEvent(player, "createCheckpoint", 3, 1, BusWays[way][0].Pos - new Vector3(0, 0, 1.12), 4, 0, 255, 0, 0, BusWays[way][1].Pos - new Vector3(0, 0, 1.12));
                Trigger.ClientEvent(player, "createWaypoint", BusWays[way][0].Pos.X, BusWays[way][0].Pos.Y);
                Trigger.ClientEvent(player, "createWorkBlip", BusWays[way][0].Pos);

                NAPI.Data.SetEntityData(player, "WORK", vehicle);

                //BasicSync.AttachLabelToObject("~y~" + BusWaysNames[way] + "\n~w~Проезд: ~g~15$", new Vector3(0, 0, 1.5), vehicle);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы арендовали автобус. Вас распределили на маршрут {BusWaysNames[way]}", 3000);
            }
            else
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в автобусе", 3000);
            }
        }

        internal class BusCheck
        {
            public Vector3 Pos { get; }
            public bool IsStop { get; }

            public BusCheck(Vector3 pos, bool isStop = false)
            {
                Pos = pos;
                IsStop = isStop;
            }
        }
    }
}
