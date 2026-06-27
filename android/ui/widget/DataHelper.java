package cn.com.heaton.shiningmask.ui.widget;

import java.util.HashMap;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class DataHelper {
    private static final int MAX_SIZE = 128;

    public static byte[] readyData(byte[] bArr) {
        byte[] bArr2 = new byte[128];
        for (int i = 0; i < 128; i++) {
            byte bAbs = (byte) Math.abs((int) bArr[i]);
            if (bAbs < 0) {
                bAbs = 127;
            }
            bArr2[i] = bAbs;
        }
        return bArr2;
    }

    public static byte[] getData(byte[] bArr) {
        byte[] bArr2 = readyData(bArr);
        return genWaveDate(bArr2, pickAltData(bArr2));
    }

    private static Map<Integer, Byte> pickAltData(byte[] bArr) {
        HashMap map = new HashMap();
        int i = 0;
        byte b = 0;
        byte b2 = 0;
        int i2 = 0;
        int i3 = 0;
        while (i < bArr.length) {
            byte b3 = bArr[i];
            if (b3 > b) {
                i2 = i;
                b = b3;
            } else if (b3 < b2) {
                if (i2 - i3 > 8) {
                    map.put(Integer.valueOf(i2), Byte.valueOf(b));
                    i3 = i2;
                }
                b = 0;
            }
            i++;
            b2 = b3;
        }
        return adornData(map);
    }

    private static Map<Integer, Byte> adornData(Map<Integer, Byte> map) {
        HashMap map2 = new HashMap();
        for (Map.Entry<Integer, Byte> entry : map.entrySet()) {
            Integer key = entry.getKey();
            double dByteValue = entry.getValue().byteValue();
            byte bRandom = (byte) (Math.random() * dByteValue);
            byte bRandom2 = (byte) (dByteValue * Math.random());
            byte bRandom3 = (byte) (((double) bRandom) * Math.random());
            byte bRandom4 = (byte) (((double) bRandom2) * Math.random());
            if (key.intValue() - 2 >= 0) {
                map2.put(Integer.valueOf(key.intValue() - 2), Byte.valueOf(bRandom3));
            }
            if (key.intValue() - 1 >= 0) {
                map2.put(Integer.valueOf(key.intValue() - 1), Byte.valueOf(bRandom));
            }
            if (key.intValue() + 1 < 128) {
                map2.put(Integer.valueOf(key.intValue() + 1), Byte.valueOf(bRandom2));
            }
            if (key.intValue() + 2 < 128) {
                map2.put(Integer.valueOf(key.intValue() + 2), Byte.valueOf(bRandom4));
            }
        }
        map.putAll(map2);
        return map;
    }

    private static byte[] genWaveDate(byte[] bArr, Map<Integer, Byte> map) {
        for (int i = 0; i < 128; i++) {
            if (map.get(Integer.valueOf(i)) == null) {
                bArr[i] = (byte) (bArr[i] / 4);
            } else {
                byte bByteValue = (byte) (((double) r1.byteValue()) * 1.5d);
                if (bByteValue < 0) {
                    bByteValue = 127;
                }
                bArr[i] = bByteValue;
            }
        }
        return bArr;
    }
}