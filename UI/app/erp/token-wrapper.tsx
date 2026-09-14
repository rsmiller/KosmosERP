"use client"

import SessionStorage from "@/components/session-storage";
import { useRouter } from 'next/navigation';
import { useEffect } from "react";

const TokenWrapper = () => {

    const router = useRouter();

    const bearerToken = SessionStorage.getToken();

    useEffect(() => {
        if(!bearerToken)
        {
            router.push("/");
        }
    }, [bearerToken, router]);

    return (
        <div></div>
        
    );
}

export default TokenWrapper;