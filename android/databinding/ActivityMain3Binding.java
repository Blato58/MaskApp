package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.viewpager.LoopViewPager;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityMain3Binding implements ViewBinding {
    public final LinearLayout layoutDots;
    public final LoopViewPager myviewpager;
    public final RelativeLayout relativelayout;
    private final LinearLayout rootView;

    private ActivityMain3Binding(LinearLayout linearLayout, LinearLayout linearLayout2, LoopViewPager loopViewPager, RelativeLayout relativeLayout) {
        this.rootView = linearLayout;
        this.layoutDots = linearLayout2;
        this.myviewpager = loopViewPager;
        this.relativelayout = relativeLayout;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ActivityMain3Binding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityMain3Binding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_main3, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityMain3Binding bind(View view) {
        int i = R.id.layout_dots;
        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
        if (linearLayout != null) {
            i = R.id.myviewpager;
            LoopViewPager loopViewPager = (LoopViewPager) ViewBindings.findChildViewById(view, i);
            if (loopViewPager != null) {
                i = R.id.relativelayout;
                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                if (relativeLayout != null) {
                    return new ActivityMain3Binding((LinearLayout) view, linearLayout, loopViewPager, relativeLayout);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}