import PropTypes from 'prop-types';
import React, { useCallback, useState } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModal from 'Components/Table/TableOptions/TableOptionsModal';
import TablePager from 'Components/Table/TablePager';
import { align, icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import LogsTableRow from './LogsTableRow';

function LogsTable(props) {
  const {
    isFetching,
    isPopulated,
    error,
    items,
    columns,
    selectedFilterKey,
    filters,
    totalRecords,
    clearLogExecuting,
    onRefreshPress,
    onClearLogsPress,
    onFilterSelect,
    ...otherProps
  } = props;

  const [isTableOptionsModalOpen, setIsTableOptionsModalOpen] = useState(false);

  const handleTableOptionsPress = useCallback(() => {
    setIsTableOptionsModalOpen(true);
  }, []);

  const handleTableOptionsModalClose = useCallback(() => {
    setIsTableOptionsModalOpen(false);
  }, []);

  return (
    <PageContent title={translate('Logs')}>
      <PageToolbar>
        <ToolbarItem
          id="refresh"
          priority={1}
          groupId="left"
          label={translate('Refresh')}
          iconName={icons.REFRESH}
          spinningName={icons.REFRESH}
          isSpinning={isFetching}
          onPress={onRefreshPress}
        />

        <ToolbarItem
          id="clear"
          priority={1}
          groupId="left"
          label={translate('Clear')}
          iconName={icons.CLEAR}
          isSpinning={clearLogExecuting}
          onPress={onClearLogsPress}
        />

        <PageToolbarSpacer />

        <ToolbarItem
          id="options"
          priority={2}
          groupId="right"
          label={translate('Options')}
          iconName={icons.TABLE}
          onPress={handleTableOptionsPress}
        />

        <ToolbarItem id="filter" pinned={true}>
          <FilterMenu
            alignMenu={align.RIGHT}
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={[]}
            onFilterSelect={onFilterSelect}
          />
        </ToolbarItem>
      </PageToolbar>

      <PageContentBody>
        <PageHeading scope={translate('System')} title={translate('Events')} />

        {
          isFetching && !isPopulated &&
            <LoadingIndicator />
        }

        {
          isPopulated && !error && !items.length &&
            <Alert kind={kinds.INFO}>
              {translate('NoEventsFound')}
            </Alert>
        }

        {
          isPopulated && !error && !!items.length &&
            <div>
              <Table
                columns={columns}
                canModifyColumns={false}
                {...otherProps}
              >
                <TableBody>
                  {
                    items.map((item) => {
                      return (
                        <LogsTableRow
                          key={item.id}
                          columns={columns}
                          {...item}
                        />
                      );
                    })
                  }
                </TableBody>
              </Table>

              <TablePager
                totalRecords={totalRecords}
                isFetching={isFetching}
                {...otherProps}
              />
            </div>
        }
      </PageContentBody>

      <TableOptionsModal
        {...otherProps}
        isOpen={isTableOptionsModalOpen}
        columns={columns}
        canModifyColumns={false}
        onModalClose={handleTableOptionsModalClose}
      />
    </PageContent>
  );
}

LogsTable.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  totalRecords: PropTypes.number,
  clearLogExecuting: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onClearLogsPress: PropTypes.func.isRequired
};

export default LogsTable;
