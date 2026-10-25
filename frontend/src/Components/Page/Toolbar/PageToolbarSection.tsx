// Compatibility layer: Sonarr v5 dropped PageToolbarSection in favour of flat
// ToolbarItem / PageToolbarSpacer children inside PageToolbar. Gamarr pages that
// have not been rewritten yet keep using sections; this renders them through
// the v5 primitives so they get v5 buttons and overflow handling.
import React, {
  Children,
  isValidElement,
  ReactElement,
  ReactNode,
  useRef,
} from 'react';
import { Align } from 'Helpers/Props/align';
import PageToolbarButton, { PageToolbarButtonProps } from './PageToolbarButton';
import PageToolbarSpacer from './PageToolbarSpacer';
import ToolbarItem from './ToolbarItem';

export interface PageToolbarSectionProps {
  children?: ReactNode;
  alignContent?: Align;
  collapseButtons?: boolean;
}

let sectionId = 0;

function PageToolbarSection({
  children,
  alignContent = 'left',
}: PageToolbarSectionProps) {
  const prefix = useRef(`section-${sectionId++}`).current;

  return (
    <>
      {alignContent === 'right' ? <PageToolbarSpacer /> : null}

      {Children.map(children, (child, index) => {
        if (!isValidElement(child)) {
          return child;
        }

        if (child.type === PageToolbarButton) {
          const { children: _children, ...props } = (
            child as ReactElement<PageToolbarButtonProps>
          ).props;

          return (
            <ToolbarItem {...props} id={`${prefix}-${index}-${props.label}`} />
          );
        }

        return child;
      })}
    </>
  );
}

export default PageToolbarSection;
