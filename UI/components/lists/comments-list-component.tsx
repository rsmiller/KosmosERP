import "@/app/styles/comments-list.component.css";

import { useParams } from "next/navigation";
import SessionStorage from "../session-storage";
import { useEffect, useState } from "react";
import { CommentDto, CommentFindCommand, CommentCreateCommand } from "@/models/comment-models";
import { commentService } from "@/services/comment-service";
import { useKeycloak } from '@react-keycloak/web';

function CommentsListComponent() {
    const params = useParams();
    const { keycloak } = useKeycloak();

    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const [loading, setLoading] = useState(true);
    const [comments, setComments] = useState<CommentDto[]>([]);
    const [newCommentText, setNewCommentText] = useState("");
    const [submitting, setSubmitting] = useState(false);

    const loadComments = () => {
        setLoading(true);
        const object_id = String(params.id);

        if(!object_id || object_id == "")
            return;

        let find_command = new CommentFindCommand();
        find_command.object_guid = object_id;

        

        commentService.find(find_command, keycloak?.token || "").then( (response) => 
        {
            setLoading(false);

            if(response.success && response.data)
            {
                const formattedComments = response.data.map(comment => ({
                    comment_by_name: comment.comment_by_name,
                    comment_text: comment.comment_text,
                    created_on: comment.created_on,
                } as CommentDto));
                
                setComments(formattedComments);
            }
        });
    };

    useEffect(() => {
        if(keycloak.authenticated == false) return;

        loadComments();
    }, [params.id, keycloak.authenticated]);

    const submitNewComment = async () => {
        if (!newCommentText.trim() || !userId || !sessionId) return;

        setSubmitting(true);
        const object_id = String(params.id);

        try {
            const createCommand = new CommentCreateCommand();
            createCommand.object_guid = object_id;
            createCommand.comment_text = newCommentText.trim();

            const response = await commentService.create(createCommand, keycloak?.token || "");
            
            if (response.success) {
                setNewCommentText("");
                // Refresh comments to include the new one
                loadComments();
            }
        } catch (error) {
            console.error("Error creating comment:", error);
        } finally {
            setSubmitting(false);
        }
    };

    const formatDate = (dateString: string) => {
        if (!dateString) return "";
        const date = new Date(dateString);
        return date.toLocaleDateString() + " " + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    };
    
    return (
        <div className="comments-container">
            <h3 className="text-lg font-semibold mb-4">Comments</h3>
            
            {/* Comment Form */}
            <div className="comment-form mb-6 p-4 border rounded-lg bg-gray-50">
                <div className="flex gap-2">
                    <textarea
                        value={newCommentText}
                        onChange={(e) => setNewCommentText(e.target.value)}
                        placeholder="Add a comment..."
                        className="flex-1 p-2 border rounded-md resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
                        rows={3}
                        disabled={submitting}
                    />
                    <button
                        onClick={submitNewComment}
                        disabled={!newCommentText.trim() || submitting}
                        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
                    >
                        {submitting ? "Posting..." : "Post Comment"}
                    </button>
                </div>
            </div>

            {/* Comments List */}
            {loading ? (
                <div className="text-center py-4 text-gray-500">Loading comments...</div>
            ) : comments.length === 0 ? (
                <div className="text-center py-4 text-gray-500">No comments yet. Be the first to comment!</div>
            ) : (
                <div className="comments-list space-y-4">
                    {comments.map((comment, index) => (
                        <div key={index} className="comment-item p-4 border rounded-lg bg-white">
                            <div className="comment-header flex justify-between items-start mb-2">
                                <span className="font-medium text-gray-900">
                                    {comment.comment_by_name || "Unknown User"}
                                </span>
                                <span className="text-sm text-gray-500">
                                    {formatDate(comment.created_on || "")}
                                </span>
                            </div>
                            <div className="comment-text text-gray-700 whitespace-pre-wrap">
                                {comment.comment_text || ""}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default CommentsListComponent;