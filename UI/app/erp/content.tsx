"use client"

import React from 'react';
import { useEffect, useState } from "react";
import { Provider } from "@/components/ui/provider"
import { Box, Flex, Text, Menu, Portal, HStack, Grid, GridItem, Avatar } from '@chakra-ui/react';
import SidebarComponent from './sidebar';
import { useKeycloak } from "@react-keycloak/web";
import SessionStorage from '@/components/session-storage';
import { usePathname } from 'next/navigation';

function ContentComponent({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) 
{
    const pathname = usePathname();
    const [ userFullName, setUserFullName ] = useState<string>("");

    const { keycloak } = useKeycloak();

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
        //return;
        
        if (!keycloak) {
            setIsLoading(true);
            return;
        }
        /*
        console.log('Keycloak authenticated, token info:', {
            authenticated: keycloak.authenticated,
            hasToken: !!keycloak.token,
            exp: keycloak.tokenParsed?.exp,
            name: keycloak.tokenParsed?.name
        });*/

        if (!keycloak.authenticated) {
            return;
        }


        setUserFullName(keycloak.tokenParsed?.name || "");
        setTimeout(() => { setIsLoading(false); }, 2000);
        

        SessionStorage.setToken(keycloak.token || "");
        SessionStorage.setName(keycloak.tokenParsed?.name || "");

        // Refresh token proactively - check every 30 seconds, refresh if < 60s remaining
        const refreshInterval = setInterval(() => {

            if (keycloak.authenticated) {
                keycloak
                    .updateToken(120)
                    .then((refreshed) => {
                        if (refreshed) {
                            if(keycloak.token != SessionStorage.getToken())
                            {
                                SessionStorage.setToken(keycloak.token || "");
                                SessionStorage.setName(keycloak.tokenParsed?.name || "")
                            }

                            //console.log('Token refreshed successfully');
                        }
                    })
                    .catch((error) => {
                        SessionStorage.removeName();
                        SessionStorage.removeToken();

                        keycloak.logout({ redirectUri: window.location.origin });
                    });
            }
        }, 30000); // check every 30s

        return () => clearInterval(refreshInterval);

    }, [keycloak?.authenticated]);

    const doLogoff = () =>
    {
        SessionStorage.removeName();
        SessionStorage.removeToken();
        keycloak?.logout({ redirectUri: window.location.origin });
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