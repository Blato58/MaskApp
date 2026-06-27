package cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager;

import android.view.View;
import cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager;

/* JADX INFO: loaded from: classes.dex */
public class CarouselZoomPostLayoutListener implements CarouselLayoutManager.PostLayoutListener {
    @Override // cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager.CarouselLayoutManager.PostLayoutListener
    public ItemTransformation transformChild(View view, float f, int i) {
        float f2 = (float) (((((-StrictMath.atan(((double) Math.abs(f)) + 1.0d)) * 2.0d) / 3.141592653589793d) + 1.0d) * 2.0d);
        return new ItemTransformation(f2, f2, Math.signum(f) * ((view.getMeasuredWidth() * (1.0f - f2)) / 1.0f), 0.0f);
    }
}