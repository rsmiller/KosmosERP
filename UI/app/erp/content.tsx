"use client"

import React from 'react';
import { useEffect, useState } from "react";
import { Provider } from "@/components/ui/provider"
import { Box, Flex, Text, Menu, Portal, HStack, Grid, GridItem, Avatar } from '@chakra-ui/react';
import SidebarComponent from './sidebar';
import { useAuth } from "@/lib/auth/auth-context";
import { usePathname, useRouter } from 'next/navigation';

function ContentComponent({
  children,
}: Readonly<{
  children: React.ReactNode;
}>)
{
    const pathname = usePathname();
    const router = useRouter();
    const [ userFullName, setUserFullName ] = useState<string>("");

    const { ready, authenticated, name, logout } = useAuth();

    const [isLoading, setIsLoading] = useState<boolean>(true);

    /*
    useEffect(() => {
        const handler = (e: CustomEvent) => {
            console.log("-------------------- Navigation event detected: " + e.detail.url);
            setIsLoading(true);
            setTimeout(() => {
                setIsLoading(false);
            }, 1200);
        };

        window.addEventListener("app:navigate", handler as EventListener);

        return () => window.removeEventListener("app:navigate", handler as EventListener);
    }, []);*/


    useEffect(() => {
        // Wait for the active auth provider to finish initializing.
        if (!ready) {
            setIsLoading(true);
            return;
        }

        // Provider-agnostic route guard: no session -> back to the landing page.
        if (!authenticated) {
            router.push('/');
            return;
        }

        setUserFullName(name || "");
        setIsLoading(false);

    }, [ready, authenticated, name]);

    const doLogoff = () =>
    {
        logout();
    }

    const navigating = () => {
        setIsLoading(true);
    }

    return (
        <div>
            <HStack align="stretch" gap={2}>
                <Box
                    w="100%"
                    color="black"
                    p={6}
                    boxShadow="sm"
                    position="sticky"
                    top={0}
                    h="75px"
                >
                    <Grid
                    templateColumns="repeat(12, 1fr)"
                    gap={6}
                    display="grid"
                    width="100%"
                    >
                    <GridItem colSpan={11}>
                        <Text fontWeight="bold" fontSize="xl" mb={6}>Kosmos ERP</Text>
                    </GridItem>
                    
                    <GridItem colSpan={1}>
                        <Menu.Root>
                        <Menu.Trigger asChild>
                            <Box cursor="pointer">
                                <Avatar.Root colorPalette="blue" justifySelf="end">
                                <Avatar.Fallback name={userFullName} />
                                </Avatar.Root>
                            </Box>
                            </Menu.Trigger>

                            <Portal>
                            <Menu.Positioner>
                                <Menu.Content>
                                <Menu.Item value="logout" onClick={doLogoff}>Logout</Menu.Item>
                                </Menu.Content>
                            </Menu.Positioner>
                            </Portal>
                        </Menu.Root>
                    </GridItem>
                    </Grid>
                </Box>
                    
            </HStack>
            <div className="pageLoadingContent" hidden={!isLoading}>
                <span className="loader"></span>
            </div>
            <Flex minH="92vh"  hidden={isLoading}>
                <Box
                w="240px"
                color="black"
                p={5}
                boxShadow="lg"
                position="sticky"
                top={0}
                h="calc(100vh-75px)"
                >
                <SidebarComponent/>
                </Box>
                <Box flex={1} p={8} bg="gray.50" className="maincontent">
                    <Provider>{children}</Provider>
                </Box>
            </Flex>
        </div>
    );
}

export default ContentComponent;