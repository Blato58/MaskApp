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
import cn.com.heaton.shiningmask.ui.widget.ImageSaveLedView;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityImageSaveBinding implements ViewBinding {
    public final FrameLayout flChange;
    public final ImageView ivAnim;
    public final ImageView ivGallery;
    public final ImageSaveLedView ledviewSave;
    public final LinearLayout llLedView1;
    public final LinearLayout rlBottom;
    private final RelativeLayout rootView;

    private ActivityImageSaveBinding(RelativeLayout relativeLayout, FrameLayout frameLayout, ImageView imageView, ImageView imageView2, ImageSaveLedView imageSaveLedView, LinearLayout linearLayout, LinearLayout linearLayout2) {
        this.rootView = relativeLayout;
        this.flChange = frameLayout;
        this.ivAnim = imageView;
        this.ivGallery = imageView2;
        this.ledviewSave = imageSaveLedView;
        this.llLedView1 = linearLayout;
        this.rlBottom = linearLayout2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityImageSaveBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityImageSaveBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_image_save, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityImageSaveBinding bind(View view) {
        int i = R.id.fl_change;
        FrameLayout frameLayout = (FrameLayout) ViewBindings.findChildViewById(view, i);
        if (frameLayout != null) {
            i = R.id.iv_anim;
            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView != null) {
                i = R.id.iv_gallery;
                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView2 != null) {
                    i = R.id.ledview_save;
                    ImageSaveLedView imageSaveLedView = (ImageSaveLedView) ViewBindings.findChildViewById(view, i);
                    if (imageSaveLedView != null) {
                        i = R.id.ll_ledView1;
                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                        if (linearLayout != null) {
                            i = R.id.rl_bottom;
                            LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                            if (linearLayout2 != null) {
                                return new ActivityImageSaveBinding((RelativeLayout) view, frameLayout, imageView, imageView2, imageSaveLedView, linearLayout, linearLayout2);
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}