package cn.com.heaton.shiningmask.ui.widget.camera;

import android.util.ArrayMap;
import java.util.Set;
import java.util.SortedSet;
import java.util.TreeSet;

/* JADX INFO: loaded from: classes.dex */
public class SizeMap {
    private final ArrayMap<AspectRatio, SortedSet<Size>> mRatios = new ArrayMap<>();

    public boolean add(Size size) {
        for (AspectRatio aspectRatio : this.mRatios.keySet()) {
            if (aspectRatio.matches(size)) {
                SortedSet<Size> sortedSet = this.mRatios.get(aspectRatio);
                if (sortedSet.contains(size)) {
                    return false;
                }
                sortedSet.add(size);
                return true;
            }
        }
        TreeSet treeSet = new TreeSet();
        treeSet.add(size);
        this.mRatios.put(AspectRatio.of(size.getWidth(), size.getHeight()), treeSet);
        return true;
    }

    public void remove(AspectRatio aspectRatio) {
        this.mRatios.remove(aspectRatio);
    }

    Set<AspectRatio> ratios() {
        return this.mRatios.keySet();
    }

    SortedSet<Size> sizes(AspectRatio aspectRatio) {
        if (this.mRatios.get(aspectRatio) != null) {
            return this.mRatios.get(aspectRatio);
        }
        float fAbs = 1.0f;
        AspectRatio aspectRatio2 = aspectRatio;
        for (AspectRatio aspectRatio3 : ratios()) {
            if (Math.abs(aspectRatio.toFloat() - aspectRatio3.toFloat()) < fAbs) {
                fAbs = Math.abs(aspectRatio.toFloat() - aspectRatio3.toFloat());
                aspectRatio2 = aspectRatio3;
            }
        }
        return this.mRatios.get(aspectRatio2);
    }

    void clear() {
        this.mRatios.clear();
    }

    boolean isEmpty() {
        return this.mRatios.isEmpty();
    }
}