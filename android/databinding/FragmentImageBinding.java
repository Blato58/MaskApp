package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class FragmentImageBinding implements ViewBinding {
    public final FrameLayout flChange;
    public final ImageView ivDefaultImage;
    public final ImageView ivDiyImage;
    public final ImageView ivLoop;
    public final LinearLayout rlTop;
    private final RelativeLayout rootView;
    public final LayoutTitlebar1Binding top;

    private FragmentImageBinding(RelativeLayout relativeLayout, FrameLayout frameLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, LinearLayout linearLayout, LayoutTitlebar1Binding layoutTitlebar1Binding) {
        this.rootView = relativeLayout;
        this.flChange = frameLayout;
        this.ivDefaultImage = imageView;
        this.ivDiyImage = imageView2;
        this.ivLoop = imageView3;
        this.rlTop = linearLayout;
        this.top = layoutTitlebar1Binding;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static FragmentImageBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static FragmentImageBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fragment_image, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static FragmentImageBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.fl_change;
        FrameLayout frameLayout = (FrameLayout) ViewBindings.findChildViewById(view, i);
        if (frameLayout != null) {
            i = R.id.iv_default_image;
            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView != null) {
                i = R.id.iv_diy_image;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView2 != null) {
                    i = R.id.iv_loop;
                    ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView3 != null) {
                        i = R.id.rl_top;
                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                        if (linearLayout != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                            return new FragmentImageBinding((RelativeLayout) view, frameLayout, imageView, imageView2, imageView3, linearLayout, LayoutTitlebar1Binding.bind(viewFindChildViewById));
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}