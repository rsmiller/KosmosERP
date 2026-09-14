"use client"

import SessionStorage from "@/components/session-storage";
import { useParams, useRouter } from 'next/navigation';
import { useKeycloak } from '@react-keycloak/web';

function DownloadFilePage() {
    const { keycloak } = useKeycloak();
    const params = useParams();
    const router = useRouter();
    
    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();
}

export default DownloadFilePage;