package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.image3d.Image3DSwitchView;
import cn.com.heaton.shiningmask.ui.widget.image3d.Image3DView;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityMain2Binding implements ViewBinding {
    public final Image3DView image1;
    public final Image3DView image2;
    public final Image3DView image3;
    public final Image3DView image4;
    public final Image3DSwitchView imageSwitchView;
    public final ImageView ivAddDevice;
    public final ImageView ivBottom;
    public final ImageView ivCenter;
    public final ImageView ivTop;
    public final LinearLayout llCenter;
    public final RelativeLayout llSeekbar;
    public final ListView lvDevice;
    private final RelativeLayout rootView;
    public final SeekBar sbMoveLight;

    private ActivityMain2Binding(RelativeLayout relativeLayout, Image3DView image3DView, Image3DView image3DView2, Image3DView image3DView3, Image3DView image3DView4, Image3DSwitchView image3DSwitchView, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, LinearLayout linearLayout, RelativeLayout relativeLayout2, ListView listView, SeekBar seekBar) {
        this.rootView = relativeLayout;
        this.image1 = image3DView;
        this.image2 = image3DView2;
        this.image3 = image3DView3;
        this.image4 = image3DView4;
        this.imageSwitchView = image3DSwitchView;
        this.ivAddDevice = imageView;
        this.ivBottom = imageView2;
        this.ivCenter = imageView3;
        this.ivTop = imageView4;
        this.llCenter = linearLayout;
        this.llSeekbar = relativeLayout2;
        this.lvDevice = listView;
        this.sbMoveLight = seekBar;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityMain2Binding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityMain2Binding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_main2, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityMain2Binding bind(View view) {
        int i = R.id.image1;
        Image3DView image3DView = (Image3DView) ViewBindings.findChildViewById(view, i);
        if (image3DView != null) {
            i = R.id.image2;
            Image3DView image3DView2 = (Image3DView) ViewBindings.findChildViewById(view, i);
            if (image3DView2 != null) {
                i = R.id.image3;
                Image3DView image3DView3 = (Image3DView) ViewBindings.findChildViewById(view, i);
                if (image3DView3 != null) {
                    i = R.id.image4;
                    Image3DView image3DView4 = (Image3DView) ViewBindings.findChildViewById(view, i);
                    if (image3DView4 != null) {
                        i = R.id.image_switch_view;
                        Image3DSwitchView image3DSwitchView = (Image3DSwitchView) ViewBindings.findChildViewById(view, i);
                        if (image3DSwitchView != null) {
                            i = R.id.iv_add_device;
                            ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView != null) {
                                i = R.id.iv_bottom;
                                ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView2 != null) {
                                    i = R.id.iv_center;
                                    ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                                    if (imageView3 != null) {
                                        i = R.id.iv_top;
                                        ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                                        if (imageView4 != null) {
                                            i = R.id.ll_center;
                                            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                            if (linearLayout != null) {
                                                i = R.id.ll_seekbar;
                                                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout != null) {
                                                    i = R.id.lv_device;
                                                    ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                                    if (listView != null) {
                                                        i = R.id.sb_move_light;
                                                        SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                        if (seekBar != null) {
                                                            return new ActivityMain2Binding((RelativeLayout) view, image3DView, image3DView2, image3DView3, image3DView4, image3DSwitchView, imageView, imageView2, imageView3, imageView4, linearLayout, relativeLayout, listView, seekBar);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}