package cn.com.heaton.shiningmask.ui.widget.viewpager;

import android.content.Context;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import androidx.viewpager.widget.PagerAdapter;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import java.util.ArrayList;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
class MyAdapter extends PagerAdapter {
    private List<RhythmImage> mImageList;

    @Override // androidx.viewpager.widget.PagerAdapter
    public boolean isViewFromObject(View view, Object obj) {
        return view == obj;
    }

    public MyAdapter(Context context, List<RhythmImage> list) {
        new ArrayList();
        this.mImageList = list;
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public int getCount() {
        return this.mImageList.size();
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public Object instantiateItem(ViewGroup viewGroup, int i) {
        ImageView imageView = this.mImageList.get(i).getImageView();
        if (imageView.getParent() == null) {
            viewGroup.addView(imageView);
        }
        imageView.setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.widget.viewpager.MyAdapter.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
            }
        });
        return imageView;
    }

    @Override // androidx.viewpager.widget.PagerAdapter
    public void destroyItem(ViewGroup viewGroup, int i, Object obj) {
        viewGroup.removeView((View) obj);
    }
}