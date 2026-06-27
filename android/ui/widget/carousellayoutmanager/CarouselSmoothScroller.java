package cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager;

import android.content.Context;
import android.view.View;
import androidx.recyclerview.widget.LinearSmoothScroller;

/* JADX INFO: loaded from: classes.dex */
public abstract class CarouselSmoothScroller extends LinearSmoothScroller {
    protected CarouselSmoothScroller(Context context) {
        super(context);
    }

    @Override // androidx.recyclerview.widget.LinearSmoothScroller
    public int calculateDyToMakeVisible(View view, int i) {
        CarouselLayoutManager carouselLayoutManager = (CarouselLayoutManager) getLayoutManager();
        if (carouselLayoutManager == null || !carouselLayoutManager.canScrollVertically()) {
            return 0;
        }
        return carouselLayoutManager.getOffsetForCurrentView(view);
    }

    @Override // androidx.recyclerview.widget.LinearSmoothScroller
    public int calculateDxToMakeVisible(View view, int i) {
        CarouselLayoutManager carouselLayoutManager = (CarouselLayoutManager) getLayoutManager();
        if (carouselLayoutManager == null || !carouselLayoutManager.canScrollHorizontally()) {
            return 0;
        }
        return carouselLayoutManager.getOffsetForCurrentView(view);
    }
}