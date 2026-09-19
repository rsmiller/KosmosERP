"use client"

import "../globals.css";
import "../styles/sidebar.component.css";
import "react-datepicker/dist/react-datepicker.css";

import { AuthShell } from "@/lib/auth/auth-factory";
import ContentComponent from './content';

export default function ErpLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <AuthShell>
      <ContentComponent>
        {children}
      </ContentComponent>
    </AuthShell>
  );
}
