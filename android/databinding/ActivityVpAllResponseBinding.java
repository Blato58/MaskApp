package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.loopviewpager.ViewPagerAllResponse;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityVpAllResponseBinding implements ViewBinding {
    public final RelativeLayout root;
    private final RelativeLayout rootView;
    public final ViewPagerAllResponse vp;

    private ActivityVpAllResponseBinding(RelativeLayout relativeLayout, RelativeLayout relativeLayout2, ViewPagerAllResponse viewPagerAllResponse) {
        this.rootView = relativeLayout;
        this.root = relativeLayout2;
        this.vp = viewPagerAllResponse;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityVpAllResponseBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityVpAllResponseBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_vp_all_response, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityVpAllResponseBinding bind(View view) {
        int i = R.id.root;
        RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
        if (relativeLayout != null) {
            i = R.id.vp;
            ViewPagerAllResponse viewPagerAllResponse = (ViewPagerAllResponse) ViewBindings.findChildViewById(view, i);
            if (viewPagerAllResponse != null) {
                return new ActivityVpAllResponseBinding((RelativeLayout) view, relativeLayout, viewPagerAllResponse);
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}