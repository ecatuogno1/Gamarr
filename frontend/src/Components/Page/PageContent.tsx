import React, { useEffect } from 'react';
import ErrorBoundary from 'Components/Error/ErrorBoundary';
import PageContentError from './PageContentError';
import styles from './PageContent.css';

interface PageContentProps {
  className?: string;
  title: string;
  children: React.ReactNode;
}

function PageContent({
  className = styles.content,
  title,
  children,
}: PageContentProps) {
  useEffect(() => {
    document.title = title
      ? `${title} - ${window.Gamarr.instanceName}`
      : window.Gamarr.instanceName;

    return () => {
      document.title = window.Gamarr.instanceName;
    };
  }, [title]);

  return (
    <ErrorBoundary errorComponent={PageContentError}>
      <main className={className} aria-label={title}>
        {children}
      </main>
    </ErrorBoundary>
  );
}

export default PageContent;
