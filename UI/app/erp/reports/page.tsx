"use client"

import { useEffect, useRef, useState } from "react";
import {
  Box,
  Button,
  Flex,
  Heading,
  HStack,
  Input,
  Link as ChakraLink,
  NativeSelect,
  Spinner,
  Stack,
  Text,
  VStack,
} from "@chakra-ui/react";
import { FaChartBar, FaFilePdf, FaPlay } from "react-icons/fa";
import { useAuth } from "@/lib/auth/auth-context";
import { reportsService } from "@/services/reports-service";
import { ReportCatalogItem, ReportCategory, ReportOption } from "@/models/report-models";

function ReportsPage() {
  const auth = useAuth();

  const [categories, setCategories] = useState<ReportCategory[]>([]);
  const [loadingCatalog, setLoadingCatalog] = useState<boolean>(true);
  const [catalogError, setCatalogError] = useState<string | null>(null);

  const [selected, setSelected] = useState<ReportCatalogItem | null>(null);
  const [paramValues, setParamValues] = useState<Record<string, string>>({});
  // Dropdown options per parameter name, fetched from each parameter's optionsEndpoint.
  const [paramOptions, setParamOptions] = useState<Record<string, ReportOption[]>>({});

  const [viewerUrl, setViewerUrl] = useState<string | null>(null);
  const [running, setRunning] = useState<boolean>(false);
  const [downloading, setDownloading] = useState<boolean>(false);
  const [runError, setRunError] = useState<string | null>(null);

  const hasInitialized = useRef(false);
  // Track the current object URL so it can be revoked when it is replaced.
  const viewerUrlRef = useRef<string | null>(null);

  useEffect(() => {
    if (auth.authenticated === false) return;
    if (hasInitialized.current) return;
    hasInitialized.current = true;

    loadCatalog();
  }, [auth.authenticated]);

  // Revoke any outstanding object URL on unmount.
  useEffect(() => {
    return () => {
      if (viewerUrlRef.current) URL.revokeObjectURL(viewerUrlRef.current);
    };
  }, []);

  const loadCatalog = async () => {
    setLoadingCatalog(true);
    setCatalogError(null);
    try {
      const response = await reportsService.getCatalog(auth.token || "");
      if (response.success && response.data) {
        setCategories(response.data);
      } else {
        setCatalogError("Unable to load the report catalog.");
      }
    } catch (error) {
      console.error("Error loading report catalog:", error);
      setCatalogError("Unable to load the report catalog.");
    } finally {
      setLoadingCatalog(false);
    }
  };

  const setViewer = (url: string | null) => {
    if (viewerUrlRef.current) URL.revokeObjectURL(viewerUrlRef.current);
    viewerUrlRef.current = url;
    setViewerUrl(url);
  };

  const onSelectReport = (report: ReportCatalogItem) => {
    setSelected(report);
    setParamValues({});
    setParamOptions({});
    setRunError(null);
    setViewer(null);
    loadParamOptions(report);
    // Auto-run with server defaults so a click immediately shows something.
    runReport(report, {});
  };

  // Load dropdown options for any parameter that declares an optionsEndpoint.
  const loadParamOptions = async (report: ReportCatalogItem) => {
    const selectParams = report.parameters.filter((p) => p.optionsEndpoint);
    for (const param of selectParams) {
      try {
        const response = await reportsService.getOptions(param.optionsEndpoint!, auth.token || "");
        if (response.success && response.data) {
          setParamOptions((prev) => ({ ...prev, [param.name]: response.data! }));
        }
      } catch (error) {
        console.error(`Error loading options for ${param.name}:`, error);
      }
    }
  };

  const onParamChange = (name: string, value: string) => {
    setParamValues((prev) => ({ ...prev, [name]: value }));
  };

  const runReport = async (report: ReportCatalogItem, values: Record<string, string>) => {
    setRunning(true);
    setRunError(null);
    try {
      const blob = await reportsService.generateReport(report, values, "html", auth.token || "");
      setViewer(URL.createObjectURL(blob));
    } catch (error) {
      console.error("Error generating report:", error);
      setRunError("The report could not be generated. Please check the parameters and try again.");
      setViewer(null);
    } finally {
      setRunning(false);
    }
  };

  const onRunClick = () => {
    if (selected) runReport(selected, paramValues);
  };

  const onDownloadPdf = async () => {
    if (!selected) return;
    setDownloading(true);
    setRunError(null);
    try {
      const blob = await reportsService.generateReport(selected, paramValues, "pdf", auth.token || "");
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `${selected.key}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Error downloading report PDF:", error);
      setRunError("The PDF could not be generated. Please check the parameters and try again.");
    } finally {
      setDownloading(false);
    }
  };

  const inputTypeFor = (type: string) =>
    type === "date" ? "date" : type === "int" ? "number" : "text";

  return (
    <Box width="100%">
      <HStack mb={6} gap={3}>
        <FaChartBar size={22} />
        <Heading size="lg">Reports</Heading>
      </HStack>

      <Flex align="stretch" gap={6} minH="70vh">
        {/* Report catalog sidebar */}
        <Box
          w="280px"
          flexShrink={0}
          bg="white"
          borderWidth="1px"
          borderColor="gray.200"
          borderRadius="md"
          boxShadow="sm"
          p={4}
          overflowY="auto"
          maxH="80vh"
        >
          {loadingCatalog ? (
            <HStack color="gray.500">
              <Spinner size="sm" /> <Text>Loading reports…</Text>
            </HStack>
          ) : catalogError ? (
            <Text color="red.500">{catalogError}</Text>
          ) : categories.length === 0 ? (
            <Text color="gray.500">No reports available.</Text>
          ) : (
            <VStack align="stretch" gap={5}>
              {categories.map((category) => (
                <Box key={category.name}>
                  <Text
                    fontSize="xs"
                    fontWeight="bold"
                    textTransform="uppercase"
                    color="gray.500"
                    mb={2}
                  >
                    {category.name}
                  </Text>
                  <VStack align="stretch" gap={1}>
                    {category.reports.map((report) => {
                      const isActive = selected?.key === report.key;
                      return (
                        <ChakraLink
                          key={report.key}
                          onClick={() => onSelectReport(report)}
                          px={3}
                          py={2}
                          borderRadius="md"
                          fontSize="sm"
                          bg={isActive ? "blue.50" : "transparent"}
                          color={isActive ? "blue.700" : "gray.800"}
                          fontWeight={isActive ? "semibold" : "normal"}
                          _hover={{ bg: isActive ? "blue.50" : "gray.100", textDecoration: "none" }}
                        >
                          {report.name}
                        </ChakraLink>
                      );
                    })}
                  </VStack>
                </Box>
              ))}
            </VStack>
          )}
        </Box>

        {/* Report viewer */}
        <Box flex={1} minW={0}>
          {!selected ? (
            <Flex
              h="100%"
              minH="60vh"
              align="center"
              justify="center"
              borderWidth="1px"
              borderStyle="dashed"
              borderColor="gray.300"
              borderRadius="md"
              color="gray.500"
            >
              <Text>Select a report from the sidebar to get started.</Text>
            </Flex>
          ) : (
            <Stack gap={4} h="100%">
              <Box>
                <Heading size="md">{selected.name}</Heading>
                <Text color="gray.600" fontSize="sm">
                  {selected.description}
                </Text>
              </Box>

              {/* Parameters + actions */}
              <Box
                bg="white"
                borderWidth="1px"
                borderColor="gray.200"
                borderRadius="md"
                p={4}
              >
                <Flex align="flex-end" gap={4} wrap="wrap">
                  {selected.parameters.map((param) => (
                    <Box key={param.name}>
                      <Text fontSize="sm" mb={1} color="gray.700">
                        {param.label}
                        {param.required ? " *" : ""}
                      </Text>
                      {param.type === "select" ? (
                        <NativeSelect.Root size="sm" width="200px">
                          <NativeSelect.Field
                            value={paramValues[param.name] ?? ""}
                            onChange={(e) => onParamChange(param.name, e.target.value)}
                            bg="white"
                          >
                            <option value="">All</option>
                            {(paramOptions[param.name] ?? []).map((option) => (
                              <option key={option.value} value={option.value}>
                                {option.label}
                              </option>
                            ))}
                          </NativeSelect.Field>
                          <NativeSelect.Indicator />
                        </NativeSelect.Root>
                      ) : (
                        <Input
                          size="sm"
                          type={inputTypeFor(param.type)}
                          value={paramValues[param.name] ?? ""}
                          onChange={(e) => onParamChange(param.name, e.target.value)}
                          width="200px"
                          bg="white"
                        />
                      )}
                    </Box>
                  ))}
                  <HStack gap={3}>
                    <Button size="sm" colorPalette="blue" onClick={onRunClick} loading={running}>
                      <FaPlay style={{ marginRight: 6 }} /> Run report
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={onDownloadPdf}
                      loading={downloading}
                    >
                      <FaFilePdf style={{ marginRight: 6 }} /> Download PDF
                    </Button>
                  </HStack>
                </Flex>
              </Box>

              {runError && <Text color="red.500">{runError}</Text>}

              {/* Rendered report */}
              <Box
                flex={1}
                minH="55vh"
                bg="white"
                borderWidth="1px"
                borderColor="gray.200"
                borderRadius="md"
                overflow="hidden"
                position="relative"
              >
                {running && (
                  <Flex position="absolute" inset={0} align="center" justify="center" bg="whiteAlpha.700">
                    <HStack color="gray.600">
                      <Spinner size="sm" /> <Text>Generating report…</Text>
                    </HStack>
                  </Flex>
                )}
                {viewerUrl ? (
                  <iframe
                    src={viewerUrl}
                    title={selected.name}
                    style={{ width: "100%", height: "100%", minHeight: "55vh", border: "none" }}
                  />
                ) : (
                  !running && (
                    <Flex h="100%" minH="55vh" align="center" justify="center" color="gray.500">
                      <Text>No report to display yet.</Text>
                    </Flex>
                  )
                )}
              </Box>
            </Stack>
          )}
        </Box>
      </Flex>
    </Box>
  );
}

export default ReportsPage;
