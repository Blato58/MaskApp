package cn.com.heaton.shiningmask.ui.utils;

import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class RhythmDataUitls {
    private static List<Integer> list0 = new ArrayList();
    private static List<Integer> list = new ArrayList();

    public static List<Integer> getRhyData1(byte[] bArr) {
        if (bArr == null) {
            return null;
        }
        list0.clear();
        list.clear();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 5) {
                list0.add(Integer.valueOf(height4 * 2));
                list0.add(Integer.valueOf(low4 * 2));
            }
        }
        for (int size = list0.size() - 1; size >= 0; size--) {
            list.add(list0.get(size));
        }
        for (int i2 = 1; i2 <= 9; i2++) {
            list.add(list0.get(i2));
        }
        return list;
    }

    public static List<Integer> getRhyData2(byte[] bArr) {
        if (bArr == null) {
            return null;
        }
        list0.clear();
        list.clear();
        ArrayList arrayList = new ArrayList();
        ArrayList arrayList2 = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 7) {
                arrayList.add(Integer.valueOf(height4 * 2));
                arrayList.add(Integer.valueOf(low4 * 2));
            }
        }
        for (int size = arrayList.size() - 2; size >= 0; size--) {
            arrayList2.add((Integer) arrayList.get(size));
        }
        for (int i2 = 1; i2 <= 12; i2++) {
            arrayList2.add((Integer) arrayList.get(i2));
        }
        return arrayList2;
    }

    public static List<Integer> getRhyData3(byte[] bArr) {
        if (bArr == null) {
            return null;
        }
        list0.clear();
        list.clear();
        ArrayList arrayList = new ArrayList();
        ArrayList arrayList2 = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 7) {
                arrayList.add(Integer.valueOf(height4 * 2));
                arrayList.add(Integer.valueOf(low4 * 2));
            }
        }
        for (int size = arrayList.size() - 2; size >= 0; size--) {
            arrayList2.add((Integer) arrayList.get(size));
        }
        for (int i2 = 0; i2 <= 12; i2++) {
            arrayList2.add((Integer) arrayList.get(i2));
        }
        return arrayList2;
    }

    public static List<Integer> getRhyData4(byte[] bArr) {
        if (bArr == null) {
            return null;
        }
        list0.clear();
        list.clear();
        ArrayList arrayList = new ArrayList();
        ArrayList arrayList2 = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 7) {
                arrayList.add(Integer.valueOf(height4 * 2));
                arrayList.add(Integer.valueOf(low4 * 2));
            }
        }
        for (int size = arrayList.size() - 2; size >= 0; size--) {
            arrayList2.add((Integer) arrayList.get(size));
        }
        for (int i2 = 0; i2 <= 12; i2++) {
            arrayList2.add((Integer) arrayList.get(i2));
        }
        return arrayList2;
    }

    public static List<Integer> getRhyData5(byte[] bArr) {
        if (bArr == null) {
            return null;
        }
        list0.clear();
        list.clear();
        ArrayList arrayList = new ArrayList();
        ArrayList arrayList2 = new ArrayList();
        for (int i = 0; i < bArr.length; i++) {
            int height4 = ByteUtils.getHeight4(bArr[i]);
            int low4 = ByteUtils.getLow4(bArr[i]);
            if (i < 5) {
                arrayList.add(Integer.valueOf(height4 * 2));
                arrayList.add(Integer.valueOf(low4 * 2));
            }
        }
        for (int size = arrayList.size() - 1; size >= 0; size--) {
            arrayList2.add((Integer) arrayList.get(size));
        }
        for (int i2 = 1; i2 <= 9; i2++) {
            arrayList2.add((Integer) arrayList.get(i2));
        }
        return arrayList2;
    }
}