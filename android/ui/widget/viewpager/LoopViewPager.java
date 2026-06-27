package cn.com.heaton.shiningmask.ui.widget.viewpager;

import android.content.Context;
import android.os.Handler;
import android.os.Message;
import android.util.AttributeSet;
import android.util.DisplayMetrics;
import android.view.WindowManager;
import android.view.animation.Interpolator;
import android.widget.Scroller;
import android.widget.TextView;
import androidx.viewpager.widget.PagerAdapter;
import androidx.viewpager.widget.ViewPager;
import java.lang.ref.WeakReference;
import java.lang.reflect.Field;
import java.util.List;
import java.util.concurrent.atomic.AtomicBoolean;

/* JADX INFO: loaded from: classes.dex */
public class LoopViewPager extends ViewPager {
    private static final int HANDLE_LOOP_MSG = 101;
    private AtomicBoolean isAutoLoop;
    private LoopPagerAdapterWrapper mAdapter;
    private MyHandler mHandler;
    ViewPager.OnPageChangeListener mOuterPageChangeListener;
    private ViewPager.OnPageChangeListener onPageChangeListener;
    private List<TextView> txtPoints;

    public void setTxtPoints(List<TextView> list) {
        this.txtPoints = list;
    }

    public void changePoints(int i) {
        if (this.txtPoints != null) {
            for (int i2 = 0; i2 < this.txtPoints.size(); i2++) {
            }
        }
    }

    @Override // androidx.viewpager.widget.ViewPager
    public void setAdapter(PagerAdapter pagerAdapter) {
        LoopPagerAdapterWrapper loopPagerAdapterWrapper = new LoopPagerAdapterWrapper(pagerAdapter);
        this.mAdapter = loopPagerAdapterWrapper;
        super.setAdapter(loopPagerAdapterWrapper);
        this.isAutoLoop.set(false);
        setCurrentItem(0, false);
    }

    @Override // androidx.viewpager.widget.ViewPager
    public PagerAdapter getAdapter() {
        LoopPagerAdapterWrapper loopPagerAdapterWrapper = this.mAdapter;
        return loopPagerAdapterWrapper != null ? loopPagerAdapterWrapper.getRealAdapter() : loopPagerAdapterWrapper;
    }

    public int getRealItem() {
        LoopPagerAdapterWrapper loopPagerAdapterWrapper = this.mAdapter;
        if (loopPagerAdapterWrapper != null) {
            return loopPagerAdapterWrapper.toRealPosition(super.getCurrentItem());
        }
        return 0;
    }

    @Override // androidx.viewpager.widget.ViewPager
    public void setCurrentItem(int i, boolean z) {
        super.setCurrentItem(this.mAdapter.toInnerPosition(i), z);
    }

    @Override // androidx.viewpager.widget.ViewPager
    public void setOnPageChangeListener(ViewPager.OnPageChangeListener onPageChangeListener) {
        this.mOuterPageChangeListener = onPageChangeListener;
    }

    public LoopViewPager(Context context) {
        super(context);
        this.isAutoLoop = new AtomicBoolean();
        this.onPageChangeListener = new ViewPager.OnPageChangeListener() { // from class: cn.com.heaton.shiningmask.ui.widget.viewpager.LoopViewPager.2
            private float mPreviousOffset = -1.0f;
            private float mPreviousPosition = -1.0f;

            @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
            public void onPageSelected(int i) {
                int realPosition = LoopViewPager.this.mAdapter.toRealPosition(i);
                LoopViewPager.this.changePoints(realPosition);
                float f = realPosition;
                if (this.mPreviousPosition != f) {
                    this.mPreviousPosition = f;
                    if (LoopViewPager.this.mOuterPageChangeListener != null) {
                        LoopViewPager.this.mOuterPageChangeListener.onPageSelected(realPosition);
                    }
                }
            }

            @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
            public void onPageScrolled(int i, float f, int i2) {
                if (LoopViewPager.this.mAdapter != null) {
                    int realPosition = LoopViewPager.this.mAdapter.toRealPosition(i);
                    if (f == 0.0f && this.mPreviousOffset == 0.0f && (i == 0 || i == LoopViewPager.this.mAdapter.getCount() - 1)) {
                        LoopViewPager.this.setCurrentItem(realPosition, false);
                    }
                    i = realPosition;
                }
                this.mPreviousOffset = f;
                if (LoopViewPager.this.mOuterPageChangeListener != null) {
                    if (i != LoopViewPager.this.mAdapter.getRealCount() - 1) {
                        LoopViewPager.this.mOuterPageChangeListener.onPageScrolled(i, f, i2);
                    } else if (f > 0.5d) {
                        LoopViewPager.this.mOuterPageChangeListener.onPageScrolled(0, 0.0f, 0);
                    } else {
                        LoopViewPager.this.mOuterPageChangeListener.onPageScrolled(i, 0.0f, 0);
                    }
                }
            }

            @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
            public void onPageScrollStateChanged(int i) {
                if (i != 0) {
                    if (i == 1 && LoopViewPager.this.isAutoLoop.get()) {
                        LoopViewPager.this.mHandler.removeCallbacksAndMessages(null);
                    }
                } else if (LoopViewPager.this.isAutoLoop.get()) {
                    LoopViewPager.this.mHandler.sendEmptyMessageDelayed(101, 3000L);
                }
                if (LoopViewPager.this.mOuterPageChangeListener != null) {
                    LoopViewPager.this.mOuterPageChangeListener.onPageScrollStateChanged(i);
                }
            }
        };
        init();
    }

    public LoopViewPager(Context context, AttributeSet attributeSet) {
        super(context, attributeSet);
        this.isAutoLoop = new AtomicBoolean();
        this.onPageChangeListener = new ViewPager.OnPageChangeListener() { // from class: cn.com.heaton.shiningmask.ui.widget.viewpager.LoopViewPager.2
            private float mPreviousOffset = -1.0f;
            private float mPreviousPosition = -1.0f;

            @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
            public void onPageSelected(int i) {
                int realPosition = LoopViewPager.this.mAdapter.toRealPosition(i);
                LoopViewPager.this.changePoints(realPosition);
                float f = realPosition;
                if (this.mPreviousPosition != f) {
                    this.mPreviousPosition = f;
                    if (LoopViewPager.this.mOuterPageChangeListener != null) {
                        LoopViewPager.this.mOuterPageChangeListener.onPageSelected(realPosition);
                    }
                }
            }

            @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
            public void onPageScrolled(int i, float f, int i2) {
                if (LoopViewPager.this.mAdapter != null) {
                    int realPosition = LoopViewPager.this.mAdapter.toRealPosition(i);
                    if (f == 0.0f && this.mPreviousOffset == 0.0f && (i == 0 || i == LoopViewPager.this.mAdapter.getCount() - 1)) {
                        LoopViewPager.this.setCurrentItem(realPosition, false);
                    }
                    i = realPosition;
                }
                this.mPreviousOffset = f;
                if (LoopViewPager.this.mOuterPageChangeListener != null) {
                    if (i != LoopViewPager.this.mAdapter.getRealCount() - 1) {
                        LoopViewPager.this.mOuterPageChangeListener.onPageScrolled(i, f, i2);
                    } else if (f > 0.5d) {
                        LoopViewPager.this.mOuterPageChangeListener.onPageScrolled(0, 0.0f, 0);
                    } else {
                        LoopViewPager.this.mOuterPageChangeListener.onPageScrolled(i, 0.0f, 0);
                    }
                }
            }

            @Override // androidx.viewpager.widget.ViewPager.OnPageChangeListener
            public void onPageScrollStateChanged(int i) {
                if (i != 0) {
                    if (i == 1 && LoopViewPager.this.isAutoLoop.get()) {
                        LoopViewPager.this.mHandler.removeCallbacksAndMessages(null);
                    }
                } else if (LoopViewPager.this.isAutoLoop.get()) {
                    LoopViewPager.this.mHandler.sendEmptyMessageDelayed(101, 3000L);
                }
                if (LoopViewPager.this.mOuterPageChangeListener != null) {
                    LoopViewPager.this.mOuterPageChangeListener.onPageScrollStateChanged(i);
                }
            }
        };
        init();
    }

    private void init() {
        super.setOnPageChangeListener(this.onPageChangeListener);
        try {
            Field declaredField = ViewPager.class.getDeclaredField("mScroller");
            declaredField.setAccessible(true);
            Field declaredField2 = ViewPager.class.getDeclaredField("sInterpolator");
            declaredField2.setAccessible(true);
            declaredField.set(this, new Scroller(getContext(), (Interpolator) declaredField2.get(null)) { // from class: cn.com.heaton.shiningmask.ui.widget.viewpager.LoopViewPager.1
                @Override // android.widget.Scroller
                public void startScroll(int i, int i2, int i3, int i4, int i5) {
                    double dAbs = ((double) Math.abs(i3)) * 1300.0d;
                    LoopViewPager loopViewPager = LoopViewPager.this;
                    super.startScroll(i, i2, i3, i4, (int) (dAbs / ((double) loopViewPager.getWidth(loopViewPager.getContext()))));
                }
            });
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void autoLoop(boolean z) {
        if (this.mHandler == null) {
            this.mHandler = new MyHandler(getContext());
        }
        if (z) {
            this.mHandler.sendEmptyMessageDelayed(101, 3000L);
        } else {
            this.mHandler.removeCallbacksAndMessages(null);
        }
        this.isAutoLoop.set(z);
    }

    private class MyHandler extends Handler {
        WeakReference<Context> mWeakReference;

        public MyHandler(Context context) {
            this.mWeakReference = new WeakReference<>(context);
        }

        @Override // android.os.Handler
        public void handleMessage(Message message) {
            if (this.mWeakReference.get() != null && message.what == 101) {
                LoopViewPager.this.setCurrentItem(LoopViewPager.this.getCurrentItem() + 1);
            }
        }
    }

    public int getWidth(Context context) {
        WindowManager windowManager = (WindowManager) context.getSystemService("window");
        DisplayMetrics displayMetrics = new DisplayMetrics();
        windowManager.getDefaultDisplay().getMetrics(displayMetrics);
        return displayMetrics.widthPixels;
    }
}