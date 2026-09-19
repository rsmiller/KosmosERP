"use client"

import SessionStorage from "@/components/session-storage";
import { useParams, useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth/auth-context';

function DownloadFilePage() {
    const auth = useAuth();
    const params = useParams();
    const router = useRouter();
    
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();
}

export default DownloadFilePage;