package cn.com.heaton.shiningmask.ui.utils;

import com.alibaba.fastjson2.JSONB;
import java.io.File;
import java.io.RandomAccessFile;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import kotlin.UByte;

/* JADX INFO: loaded from: classes.dex */
public class ByteUtils {
    private static final String TAG = "ByteUtils";

    public static int getHeight4(byte b) {
        return (b & JSONB.Constants.BC_INT32_NUM_MIN) >> 4;
    }

    public static int getLow4(byte b) {
        return b & 15;
    }

    public static byte[] toBytes(short[] sArr) {
        int length = sArr.length;
        byte[] bArr = new byte[length << 1];
        for (int i = 0; i < length; i++) {
            int i2 = i * 2;
            short s = sArr[i];
            bArr[i2] = (byte) s;
            bArr[i2 + 1] = (byte) (s >> 8);
        }
        return bArr;
    }

    public static String binaryToHexString(byte[] bArr) {
        String str = "";
        for (byte b : bArr) {
            str = str + (String.valueOf("0123456789ABCDEF".charAt((b & JSONB.Constants.BC_INT32_NUM_MIN) >> 4)) + String.valueOf("0123456789ABCDEF".charAt(b & 15))) + " ";
        }
        return str;
    }

    public static byte[] toBytes(float f) {
        int iFloatToIntBits = Float.floatToIntBits(f);
        byte[] bArr = new byte[4];
        for (int i = 0; i < 4; i++) {
            bArr[i] = (byte) (iFloatToIntBits >> (24 - (i * 8)));
        }
        byte[] bArr2 = new byte[4];
        System.arraycopy(bArr, 0, bArr2, 0, 4);
        for (int i2 = 0; i2 < 2; i2++) {
            byte b = bArr2[i2];
            int i3 = 3 - i2;
            bArr2[i2] = bArr2[i3];
            bArr2[i3] = b;
        }
        return bArr2;
    }

    public static byte[] byteMerger(byte[] bArr, byte[] bArr2) {
        byte[] bArr3 = new byte[bArr.length + bArr2.length];
        System.arraycopy(bArr, 0, bArr3, 0, bArr.length);
        System.arraycopy(bArr2, 0, bArr3, bArr.length, bArr2.length);
        return bArr3;
    }

    public static byte[] toBytes(short s) {
        return new byte[]{(byte) s, (byte) (s >> 8)};
    }

    public static byte[] toBytes(int i) {
        return new byte[]{(byte) (i & 255), (byte) ((i >> 8) & 255), (byte) ((i >> 16) & 255), (byte) ((i >> 24) & 255)};
    }

    public static byte[] toBytes(String str) {
        return str.getBytes();
    }

    public static byte[] toBytes(long j) {
        ByteBuffer byteBufferAllocate = ByteBuffer.allocate(8);
        byteBufferAllocate.putLong(0, j);
        return byteBufferAllocate.array();
    }

    public static int toInt(byte[] bArr, int i) {
        return ((bArr[i + 3] & UByte.MAX_VALUE) << 24) | (bArr[i] & UByte.MAX_VALUE) | ((bArr[i + 1] & UByte.MAX_VALUE) << 8) | ((bArr[i + 2] & UByte.MAX_VALUE) << 16);
    }

    public static int toInt(byte[] bArr) {
        return toInt(bArr, 0);
    }

    public static long toLong(byte[] bArr) {
        ByteBuffer byteBufferAllocate = ByteBuffer.allocate(8);
        byteBufferAllocate.put(bArr, 0, bArr.length);
        return byteBufferAllocate.getLong();
    }

    public static short[] toShorts(byte[] bArr) {
        int length = bArr.length >> 1;
        short[] sArr = new short[length];
        for (int i = 0; i < length; i++) {
            int i2 = i * 2;
            sArr[i] = (short) (((bArr[i2 + 1] & UByte.MAX_VALUE) << 8) | (bArr[i2] & UByte.MAX_VALUE));
        }
        return sArr;
    }

    public static byte[] merger(byte[] bArr, byte[] bArr2) {
        byte[] bArr3 = new byte[bArr.length + bArr2.length];
        System.arraycopy(bArr, 0, bArr3, 0, bArr.length);
        System.arraycopy(bArr2, 0, bArr3, bArr.length, bArr2.length);
        return bArr3;
    }

    public static String toString(byte[] bArr) {
        return Arrays.toString(bArr);
    }

    public static void byte2File(byte[] bArr, File file) {
        try {
            RandomAccessFile randomAccessFile = new RandomAccessFile(file, "rw");
            try {
                randomAccessFile.seek(file.length());
                randomAccessFile.write(bArr);
                randomAccessFile.close();
            } finally {
            }
        } catch (Exception unused) {
        }
    }

    public static byte[] intToByteArrayH(int i) {
        return new byte[]{(byte) ((i >> 24) & 255), (byte) ((i >> 16) & 255), (byte) ((i >> 8) & 255), (byte) (i & 255)};
    }

    public static byte[] intToByteArrayL(int i) {
        return new byte[]{(byte) (i & 255), (byte) ((i >> 8) & 255), (byte) ((i >> 16) & 255), (byte) ((i >> 24) & 255)};
    }

    public int byteArrayToIntL(byte[] bArr) {
        int i = 0;
        for (int i2 = 0; i2 < bArr.length; i2++) {
            i += (bArr[i2] & UByte.MAX_VALUE) << (i2 * 8);
        }
        return i;
    }

    public static int byteArrayIntH(byte[] bArr) {
        int i = 0;
        for (int i2 = 0; i2 < bArr.length; i2++) {
            i += (bArr[i2] & UByte.MAX_VALUE) << ((3 - i2) * 8);
        }
        return i;
    }

    public static <T> List<List<T>> splitList(List<T> list) {
        if (list == null || list.isEmpty()) {
            return Collections.emptyList();
        }
        ArrayList arrayList = new ArrayList();
        int size = list.size();
        int i = (size + 14) / 15;
        int i2 = 0;
        while (i2 < i) {
            int i3 = i2 * 15;
            i2++;
            int i4 = i2 * 15;
            if (i4 > size) {
                i4 = size;
            }
            arrayList.add(list.subList(i3, i4));
        }
        return arrayList;
    }

    public static byte[] listConvertByteArray(List<Integer> list) {
        byte[] bArr = new byte[list.size()];
        for (int i = 0; i < list.size(); i++) {
            bArr[i] = (byte) list.get(i).intValue();
        }
        return bArr;
    }
}