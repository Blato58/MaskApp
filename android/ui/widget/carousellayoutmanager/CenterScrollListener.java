package cn.com.heaton.shiningmask.ui.widget.carousellayoutmanager;

import androidx.recyclerview.widget.RecyclerView;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;

/* JADX INFO: loaded from: classes.dex */
public class CenterScrollListener extends RecyclerView.OnScrollListener {
    public static ScrollListener scrollListener;
    private boolean mAutoSet = true;

    public interface ScrollListener {
        void onScrollSettling();

        void onStartScroll();

        void onStopScroll();
    }

    public void setScrollListener(ScrollListener scrollListener2) {
        scrollListener = scrollListener2;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.OnScrollListener
    public void onScrollStateChanged(RecyclerView recyclerView, int i) {
        super.onScrollStateChanged(recyclerView, i);
        LogUtil.d("newState:" + i);
        if (i == 0) {
            LogUtil.d("滑动停止");
            ScrollListener scrollListener2 = scrollListener;
            if (scrollListener2 != null) {
                scrollListener2.onStopScroll();
            }
        } else if (i == 1) {
            LogUtil.d("正在拖拽");
            ScrollListener scrollListener3 = scrollListener;
            if (scrollListener3 != null) {
                scrollListener3.onStartScroll();
            }
        } else if (i == 2) {
            LogUtil.d("惯性滑动中");
            ScrollListener scrollListener4 = scrollListener;
            if (scrollListener4 != null) {
                scrollListener4.onScrollSettling();
            }
        }
        RecyclerView.LayoutManager layoutManager = recyclerView.getLayoutManager();
        if (!(layoutManager instanceof CarouselLayoutManager)) {
            this.mAutoSet = true;
            return;
        }
        CarouselLayoutManager carouselLayoutManager = (CarouselLayoutManager) layoutManager;
        if (i == 0) {
            int offsetCenterView = carouselLayoutManager.getOffsetCenterView();
            if (carouselLayoutManager.getOrientation() == 0) {
                LogUtil.d(" recyclerView.smoothScrollBy:" + offsetCenterView);
                recyclerView.smoothScrollBy(offsetCenterView, 0);
            }
            this.mAutoSet = true;
        }
        if (1 == i || 2 == i) {
            this.mAutoSet = false;
        }
    }
}