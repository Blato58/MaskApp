package cn.com.heaton.shiningmask.ui.adapter.baseadapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import androidx.recyclerview.widget.RecyclerView;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public abstract class RecyclerAdapter<T> extends RecyclerView.Adapter<RecyclerViewHolder> {
    protected Context mContext;
    protected List<T> mDatas;
    protected LayoutInflater mInflater;
    protected int mLayoutId;
    private OnItemClickListener mOnItemClickListener;
    private OnItemLongClickListener mOnItemLongClickListener;

    public interface OnItemClickListener<T> {
        void onItemClick(ViewGroup viewGroup, View view, T t, int i);
    }

    public interface OnItemLongClickListener<T> {
        boolean onItemLongClick(ViewGroup viewGroup, View view, T t, int i);
    }

    public abstract void convert(RecyclerViewHolder recyclerViewHolder, T t);

    protected boolean isEnabled(int i) {
        return true;
    }

    public void setOnItemClickListener(OnItemClickListener onItemClickListener) {
        this.mOnItemClickListener = onItemClickListener;
    }

    public void setOnItemLongClickListener(OnItemLongClickListener onItemLongClickListener) {
        this.mOnItemLongClickListener = onItemLongClickListener;
    }

    public RecyclerAdapter(Context context, List<T> list) {
        this.mContext = context;
        this.mInflater = LayoutInflater.from(context);
        this.mDatas = list;
    }

    public void setList(List<T> list) {
        this.mDatas = list;
    }

    public RecyclerAdapter(Context context, int i, List<T> list) {
        this.mContext = context;
        this.mInflater = LayoutInflater.from(context);
        this.mLayoutId = i;
        this.mDatas = list;
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public RecyclerViewHolder onCreateViewHolder(ViewGroup viewGroup, int i) {
        RecyclerViewHolder recyclerViewHolder = RecyclerViewHolder.get(this.mContext, null, viewGroup, this.mLayoutId, -1);
        setListener(viewGroup, recyclerViewHolder, i);
        return recyclerViewHolder;
    }

    /* JADX INFO: Access modifiers changed from: protected */
    public int getPosition(RecyclerViewHolder recyclerViewHolder) {
        return recyclerViewHolder.getAdapterPosition();
    }

    protected void setListener(final ViewGroup viewGroup, final RecyclerViewHolder recyclerViewHolder, int i) {
        if (isEnabled(i)) {
            recyclerViewHolder.getConvertView().setOnClickListener(new View.OnClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter.1
                @Override // android.view.View.OnClickListener
                public void onClick(View view) {
                    if (RecyclerAdapter.this.mOnItemClickListener != null) {
                        int position = RecyclerAdapter.this.getPosition(recyclerViewHolder);
                        RecyclerAdapter.this.mOnItemClickListener.onItemClick(viewGroup, view, RecyclerAdapter.this.mDatas.get(position), position);
                    }
                }
            });
            recyclerViewHolder.getConvertView().setOnLongClickListener(new View.OnLongClickListener() { // from class: cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter.2
                @Override // android.view.View.OnLongClickListener
                public boolean onLongClick(View view) {
                    if (RecyclerAdapter.this.mOnItemLongClickListener == null) {
                        return false;
                    }
                    int position = RecyclerAdapter.this.getPosition(recyclerViewHolder);
                    return RecyclerAdapter.this.mOnItemLongClickListener.onItemLongClick(viewGroup, view, RecyclerAdapter.this.mDatas.get(position), position);
                }
            });
        }
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public void onBindViewHolder(RecyclerViewHolder recyclerViewHolder, int i) {
        recyclerViewHolder.updatePosition(i);
        convert(recyclerViewHolder, this.mDatas.get(i));
    }

    @Override // androidx.recyclerview.widget.RecyclerView.Adapter
    public int getItemCount() {
        if (this.mDatas.size() != 0) {
            return this.mDatas.size();
        }
        return 0;
    }
}