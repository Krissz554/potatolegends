import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AuthProvider } from "@/contexts/AuthContext";
import { Analytics } from "@vercel/analytics/react";
import Index from "./pages/Index";
import NotFound from "./pages/NotFound";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <AuthProvider>
      <TooltipProvider>
        <Toaster />
        <Sonner 
          position="bottom-right"
          toastOptions={{
            style: {
              position: 'fixed',
              bottom: '20px',
              right: '20px',
              zIndex: 9999
            }
          }}
        />
        <BrowserRouter>
          <Routes>
            {/* Main game routes - all handled by Index with persistent layout */}
            <Route path="/" element={<Index />} />
            <Route path="/collection" element={<Index />} />
            <Route path="/deck-builder" element={<Index />} />
            <Route path="/hero-hall" element={<Index />} />
            {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
        <Analytics />
      </TooltipProvider>
    </AuthProvider>
  </QueryClientProvider>
);

export default App;
