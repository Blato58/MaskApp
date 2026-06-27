package cn.com.heaton.shiningmask.ui.widget.loopviewpager;

import android.view.View;
import androidx.viewpager.widget.ViewPager;

/* JADX INFO: loaded from: classes.dex */
public class ZoomOutPageTransformer implements ViewPager.PageTransformer {
    public static final float MAX_SCALE = 1.0f;
    public static float MIN_ALPHA = 1.0f;
    public static final float MIN_SCALE = 0.7f;

    public ZoomOutPageTransformer() {
    }

    public ZoomOutPageTransformer(float f) {
        MIN_ALPHA = f;
    }

    @Override // androidx.viewpager.widget.ViewPager.PageTransformer
    public void transformPage(View view, float f) {
        if (f < -1.0f) {
            view.setScaleX(0.7f);
            view.setScaleY(0.7f);
            view.setAlpha(MIN_ALPHA);
        } else {
            if (f < 1.0f) {
                float fAbs = ((1.0f - Math.abs(f)) * 0.3f) + 0.7f;
                if (f > 0.0f) {
                    view.setTranslationX(-fAbs);
                } else if (f < 0.0f) {
                    view.setTranslationX(fAbs);
                }
                view.setScaleY(fAbs);
                view.setScaleX(fAbs);
                float f2 = MIN_ALPHA;
                view.setAlpha(f2 + ((1.0f - f2) * (1.0f - Math.abs(f))));
                return;
            }
            view.setScaleX(0.7f);
            view.setScaleY(0.7f);
            view.setAlpha(MIN_ALPHA);
        }
    }
}