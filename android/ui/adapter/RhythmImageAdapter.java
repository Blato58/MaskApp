package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.view.View;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.RhythmImage;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerViewHolder;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class RhythmImageAdapter extends RecyclerAdapter<RhythmImage> {
    private Context context;
    private OnItemClickListener onItemClickListener;

    public interface OnItemClickListener {
        void onClick(int i);
    }

    public void setOniClickListener(OnItemClickListener onItemClickListener) {
        this.onItemClickListener = onItemClickListener;
    }

    @Override // cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter
    public void convert(final RecyclerViewHolder recyclerViewHolder, RhythmImage rhythmImage) {
        recyclerViewHolder.setImageResource(R.id.iv_rhy_image, rhythmImage.getImageRes());
        recyclerViewHolder.setVisible(R.id.iv_rhy_image, rhythmImage.isShowImage());
        recyclerViewHolder.setOnClickListener(R.id.iv_icon, new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.RhythmImageAdapter.1
            @Override // android.view.View.OnClickListener
            public void onClick(View view) {
                if (RhythmImageAdapter.this.onItemClickListener != null) {
                    RhythmImageAdapter.this.onItemClickListener.onClick(RhythmImageAdapter.this.getPosition(recyclerViewHolder));
                }
            }
        });
    }

    public RhythmImageAdapter(Context context, int i, List<RhythmImage> list) {
        super(context, i, list);
        this.context = context;
    }
}